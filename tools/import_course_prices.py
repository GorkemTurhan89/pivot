# -*- coding: utf-8 -*-
"""
BÜYÜK GEÇİŞ (2026-05-30): Excel Course_Prices sayfasını DB'ye yükler.
- Country (currency = o ülkedeki en sık para birimi)
- City (Campus)
- School (chain) — okul+kampüs çiftinden tekil
- Program (Programme) — okul+programme çiftinden tekil
- PaymentPlan: her satır bir bant (MinWeek..MaxWeek + Weekly/FixedTotal fiyat)
"""
import openpyxl, pymysql, datetime
from collections import Counter, defaultdict

PATH = r"C:\Users\gorkem.turhan\Downloads\PİVOT SON DOĞRU OKUL.xlsx"

conn = pymysql.connect(host="127.0.0.1", port=3306, user="root", password="Pivot.2026!",
                       database="pivot", charset="utf8mb4", autocommit=False)
cur = conn.cursor()

wb = openpyxl.load_workbook(PATH, data_only=True)
ws = wb["Course_Prices"]

# Tüm satırları topla (header hariç, School boş olanları atla)
rows = []
for r in ws.iter_rows(min_row=2, values_only=True):
    if r[0] is None or str(r[0]).strip() == "":
        continue
    rows.append(r)
print(f"Course_Prices: {len(rows)} satır okundu.")

# 1) Country.Currency: o ülke için en sık görülen Currency.
country_currency_counter = defaultdict(Counter)
for r in rows:
    country = str(r[1]).strip()
    curr = str(r[13]).strip() if r[13] else None
    if curr:
        country_currency_counter[country][curr] += 1
country_currency = {c: cnt.most_common(1)[0][0] for c, cnt in country_currency_counter.items()}
print("Ülke -> Para birimi:", country_currency)

# 2) Insert helpers (cache by natural key)
country_id = {}
def get_country_id(name):
    if name in country_id: return country_id[name]
    cur.execute("INSERT INTO Countries (Name, Currency) VALUES (%s, %s)", (name, country_currency.get(name, "USD")))
    country_id[name] = cur.lastrowid
    return country_id[name]

city_id = {}  # (country_name, city_name) -> id
def get_city_id(country, city):
    key = (country, city)
    if key in city_id: return city_id[key]
    cur.execute("INSERT INTO Cities (Name, CountryId) VALUES (%s, %s)", (city, get_country_id(country)))
    city_id[key] = cur.lastrowid
    return city_id[key]

school_id = {}  # (country, city, school) -> id
def get_school_id(country, city, school):
    key = (country, city, school)
    if key in school_id: return school_id[key]
    cur.execute("INSERT INTO Schools (Name, CityId) VALUES (%s, %s)", (school, get_city_id(country, city)))
    school_id[key] = cur.lastrowid
    return school_id[key]

program_id = {}  # (school_id, programme) -> id
def get_program_id(country, city, school, programme):
    sid = get_school_id(country, city, school)
    key = (sid, programme)
    if key in program_id: return program_id[key]
    cur.execute("INSERT INTO Programs (Name, SchoolId) VALUES (%s, %s)", (programme, sid))
    program_id[key] = cur.lastrowid
    return program_id[key]

def to_decimal(v):
    if v is None: return None
    try:
        d = float(v)
        return d if d != 0 else d  # keep 0 as 0; None handled above
    except (TypeError, ValueError):
        return None

def to_date(v):
    if v is None or v == "": return None
    if isinstance(v, (datetime.datetime, datetime.date)):
        return v.date() if isinstance(v, datetime.datetime) else v
    try:
        return datetime.date.fromisoformat(str(v)[:10])
    except Exception:
        return None

# 3) PaymentPlan satırlarını insert et
plan_sql = """INSERT INTO PaymentPlans
(SchoolId, ProgramId, Name, MinWeek, MaxWeek, PriceType,
 WeeklyListFee, WeeklyPromoFee, TotalListFee, TotalPromoFee,
 PromoValidFrom, PromoValidUntil,
 PackageType, IsAdditional, IsOrHasMandatory, IsActive)
VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, 1, 0, 0, 1)"""

ins = 0
for r in rows:
    school   = str(r[0]).strip()
    country  = str(r[1]).strip()
    campus   = str(r[2]).strip()
    progname = str(r[3]).strip()
    min_w    = int(r[7]) if r[7] is not None else 1
    max_w    = int(r[8]) if r[8] is not None else 999
    ptype_s  = (str(r[14]).strip() if r[14] else "Weekly")
    ptype    = 1 if ptype_s == "Weekly" else 2

    list_w   = to_decimal(r[9])   if ptype == 1 else None
    promo_w  = to_decimal(r[10])  if ptype == 1 else None
    list_t   = to_decimal(r[11])  if ptype == 2 else None
    promo_t  = to_decimal(r[12])  if ptype == 2 else None
    p_from   = to_date(r[15])
    p_until  = to_date(r[16])

    sid = get_school_id(country, campus, school)
    pid = get_program_id(country, campus, school, progname)

    cur.execute(plan_sql, (sid, pid, progname, min_w, max_w, ptype,
                           list_w, promo_w, list_t, promo_t, p_from, p_until))
    ins += 1

conn.commit()
print(f"Eklenen PaymentPlan: {ins}")
print(f"Countries: {len(country_id)}, Cities: {len(city_id)}, Schools: {len(school_id)}, Programs: {len(program_id)}")
cur.close(); conn.close()
