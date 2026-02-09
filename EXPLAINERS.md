# EXPLAINERS — PricingEngine (Rp/IDR)

Dokumen ini menjelaskan formula utama yang digunakan untuk perhitungan HPP.

## Notasi
- `q_i`: kuantitas bahan i
- `p_i`: harga per unit bahan i (Rp)
- `B`: batch multiplier
- `N_theo`: output teoritis
- `w`: waste fraction

## Rumus
- **Biaya bahan**: `C_ing = Σ (p_i × q_i × B)`
- **Minyak**: `C_oil_use = o × p_oil`, `C_oil_amort = C_oil_change / B_per_oil_change`
- **Energi**: `C_energy = E_kwh × r_energy`
- **Tenaga kerja**: `C_labor = Σ (hours × rate)`
- **Packaging**: `C_pack = pack_per_unit × N_sellable`
- **Sellable**: `N_sellable = floor(N_theo × (1 − w))`
- **Total**: `C_batch = C_ing + C_oil_use + C_oil_amort + C_energy + C_labor + C_overhead + C_pack`
- **Unit cost**: `C_unit = C_batch / N_sellable`
- **Harga saran**: `P_raw = C_unit × (1 + M)`, `P_suggest = RoundTo(P_raw)`
- **Margin**: `(P_suggest − C_unit) / P_suggest`
- **PPN**: `P_incVAT = P_suggest × (1 + VAT)`

## Pembulatan
`RoundTo` mendukung aturan seperti `Nearest100`, `Nearest500`, `Nearest1000`, dan `Psychological990`.

## Catatan
- Semua nilai uang menggunakan `decimal`.
- Simpan nilai mentah tanpa pembulatan; pembulatan hanya untuk presentasi.
