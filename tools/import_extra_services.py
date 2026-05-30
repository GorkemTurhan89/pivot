# -*- coding: utf-8 -*-
"""
Vize ve Uçak Bileti.xlsx -> PaymentPlan (Category=Vize/UçakBileti, SchoolId NULL,
PriceType=FixedTotal, IsAdditional=1) + ExtraServiceDetail (Country, VisaType,
DefaultPriceText, Currency). Tutar range string'i ("£115-120") TotalListFee'ye
parse edebildiği ilk sayıyı yazılır (sales rep UI'da düzeltir).
"""
import openpyxl, pymysql, re, sys

PATH = sys.argv[1] if len(sys.argv) > 1 else r"backups/visa_flight.xlsx"

# Excel ülke isimlerini DB Country.Name'e mapla (LSI/Course_Prices'ten gelenler).
TR_TO_DB = {
    "ABD": "USA", "Amerika": "USA",
    "UK": "UK", "İngiltere": "UK",
    "İrlanda": "Ireland",
    "Malta": "Malta",
    "Almanya": "Germany",
    "Kanada": "Canada",
    "Avustralya": "Australia",
    "Fransa": "France",
    "Dubai": "Dubai",
}

# Tutar string'inden currency çıkar
CURRENCY_PATTERNS = [
    (r"\bUSD\b|\$", "USD"),
    (r"\bGBP\b|£",  "GBP"),
    (r"\bEUR\b|€|\bEuro\b", "EUR"),
    (r"\bCAD\b",   "CAD"),
    (r"\bAUD\b",   "AUD"),
]

def parse_amount_currency(text, fallback_currency):
    if text is None: return (None, fallback_currency)
    s = str(text).strip()
    # currency tespit
    currency = fallback_currency
    for pat, code in CURRENCY_PATTERNS:
        if re.search(pat, s, re.IGNORECASE):
            currency = code; break
    # ilk sayıyı yakala ("115-120" -> 115, "2,000+" -> 2000, "558" -> 558)
    m = re.search(r"[\d.,]+", s.replace(",", ""))
    amount = None
    if m:
        try: amount = float(m.group())
        except: amount = None
    return (amount, currency)

# Ülkeye göre fallback currency
COUNTRY_CURRENCY = {
    "USA": "USD", "UK": "GBP", "Ireland": "EUR", "Malta": "EUR",
    "Germany": "EUR", "Canada": "CAD", "Australia": "AUD",
    "France": "EUR", "Dubai": "USD",
}

conn = pymysql.connect(host="127.0.0.1", port=3306, user="root", password="Pivot.2026!",
                       database="pivot", charset="utf8mb4", autocommit=False)
cur = conn.cursor()

wb = openpyxl.load_workbook(PATH, data_only=True)
ws = wb["Vize Masraflari"]

plan_sql = """INSERT INTO PaymentPlans
(SchoolId, ProgramId, Name, MinWeek, MaxWeek, PriceType,
 TotalListFee, PackageType, IsAdditional, Category, IsOrHasMandatory, IsActive)
VALUES (NULL, NULL, %s, 1, 1, 2, %s, 2, 1, %s, 0, 1)"""

detail_sql = """INSERT INTO ExtraServiceDetails
(PaymentPlanId, Country, VisaType, DefaultPriceText, Currency)
VALUES (%s, %s, %s, %s, %s)"""

ins = 0; skipped = 0
for r in ws.iter_rows(min_row=2, values_only=True):
    if r[0] is None or str(r[0]).strip() == "":
        skipped += 1; continue
    country_tr = str(r[0]).strip()
    visa_type  = (str(r[1]).strip() if r[1] else None) or None
    item       = str(r[2]).strip() if r[2] else ""
    price_text = str(r[3]).strip() if r[3] else ""
    if not item:
        skipped += 1; continue

    country = TR_TO_DB.get(country_tr, country_tr)
    fallback = COUNTRY_CURRENCY.get(country, "USD")
    amount, currency = parse_amount_currency(price_text, fallback)

    category = "UçakBileti" if "Uçak" in item or "Bilet" in item else "Vize"
    name = item if visa_type is None else f"{item} ({visa_type})"

    cur.execute(plan_sql, (name, amount, category))
    pid = cur.lastrowid
    cur.execute(detail_sql, (pid, country, visa_type, price_text, currency))
    ins += 1

conn.commit()
print(f"ExtraService eklenen: {ins} | Skip: {skipped}")
cur.close(); conn.close()
