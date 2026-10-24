# Garden of Time — migration project context

## Goal
Migrate content from Google Sites export (`reference-site/*.html`) into the new brochure-style static site (`src/GardenOfTime.Web/wwwroot/*.html`), page by page. User likes the clean "brosură" style — keep it.

## Key facts
- Deployed baseline: https://wonderful-moss-0448b2d10.2.azurestaticapps.net/ — `index.html` (hero-only landing) must stay byte-identical to it. Hero image = Unsplash photo-1500382017468 in `styles.css` `.hero-image`.
- `gradina.html` = main content page (sections 01–07: despre, povestea, pauza/culinare/senzoriale 4-card grids, pentru cine, mărturii, rezervare). MERGED to `main` via PR #7 (merge commit e6f8927, 2026-10-23). Start fresh branches from `main`.
- Preview: `python3 -m http.server 8080` from wwwroot (async terminal); VS Code forwards the port; user hard-refreshes with Ctrl+Shift+R.

## Done: aleea-terapeutica + experiente-in-gradina (2026-10-24)
- Migrated into `experiente.html`: 01 aleea intro (potcoavă, mers desculț), 02 texturi senzoriale (dark `.path-section`, Kneipp), 03 beneficii (`.audience-list`), 04–11 the 8 experience sections (perseide, cicloturism, ateliere plante, degustări vin, cinema sub stele, drumeții, plăcinte sălăjene + list, baschet), rezervare CTA.
- Old placeholder grid (stock photos) removed — all photos now real garden photos.
- New assets: `alee-terapeutica-traseu.jpg`, `alee-terapeutica-descult.jpg`, `perseide-cer-instelat.jpg`, `cicloturism-vama-buzaului.jpg`, `atelier-plante-medicinale.jpg`, `degustare-vin-apus.jpg`, `cinema-sub-stele.jpg`, `drumetie-munte-vama.jpg`, `placinte-salajene-brunch.jpg`, `baschet-in-gradina.png`.

## Done: obiective-turistice + cum-poti-ajunge + evenimente + cazare (2026-10-24)
- New page `imprejurimi.html` (nav "Împrejurimi", added to all pages except index.html which stays byte-identical): 01 OBIECTIVE TURISTICE (8 quote cards, video on top, cite→valea-zimbrilor.ro), 02 EVENIMENTE (4-card grid: Vama pe bicicletă, Festivalul Național de Folk, Gospodarul Vămăsan, Târgul de Brânză), 03 CAZARE (The Valley Resort + Vama 54 quote-video cards, link to valea-zimbrilor.ro/cazare), rezervare CTA.
- `cum-ajungi.html`: 01 location map, 02 CUM AJUNGI (4 transport modes as `.audience-list`: 🚗 Brașov, 🚌 linia 540+541, 🚂 tren Întorsura Buzăului, 🛣️ București DN1+DJ103B; links ratbv.ro + mersultrenurilor.infofer.ro).
- YouTube embeds: use `www.youtube.com/embed` + `referrerpolicy="strict-origin-when-cross-origin"` (youtube-nocookie throws Error 153).
- New CSS: `.testimonial-grid.two-cols` (2-col, flex cards, cite pinned bottom), `.testimonial iframe`, `.testimonial cite a`, `.intro-copy .text-link + .text-link`.

## Next: contact-rezervari* (contact, robotelul-gradinii, formular, tarife)
- Then: utile* remaining (vremea, cand-e-cel-mai-bine).

## Reference-site handling
- Google Sites HTML is heavy markup with long lines; extract text with python `html.parser` (skip script/style).
- Map images to sections by walking HTML in document order (text + IMG events); image files are hash-named in `reference-site/assets/`.
- Match reference images to wwwroot copies via `md5sum` or file size.

## Image conventions (wwwroot/assets/)
- Descriptive Romanian names (e.g. `banca-cafea-flori.jpg`, `alee-terapeutica-start.jpg`).
- User renames files to match ACTUAL photo content — after renames, fix references in HTML (map old→new by size/md5) and update alt texts/captions that clash.
- Prefer real garden photos from reference-site over Unsplash stock.
- Do not trust my own image-viewing guesses for content; user's filenames are ground truth.

## Verification
- `curl` each referenced image for HTTP 200; `diff` against deployed files for rollbacks.
- Content language: Romanian with correct diacritics (ă, â, î, ș, ț). Keep reference phrasing recognizable in captions.
