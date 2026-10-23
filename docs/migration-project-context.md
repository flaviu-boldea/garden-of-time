# Garden of Time — migration project context

## Goal
Migrate content from Google Sites export (`reference-site/*.html`) into the new brochure-style static site (`src/GardenOfTime.Web/wwwroot/*.html`), page by page. User likes the clean "brosură" style — keep it.

## Key facts
- Deployed baseline: https://wonderful-moss-0448b2d10.2.azurestaticapps.net/ — `index.html` (hero-only landing) must stay byte-identical to it. Hero image = Unsplash photo-1500382017468 in `styles.css` `.hero-image`.
- `gradina.html` = main content page (sections 01–07: despre, povestea, pauza/culinare/senzoriale 4-card grids, pentru cine, mărturii, rezervare). MERGED to `main` via PR #7 (merge commit e6f8927, 2026-10-23). Start fresh branches from `main`.
- Preview: `python3 -m http.server 8080` from wwwroot (async terminal); VS Code forwards the port; user hard-refreshes with Ctrl+Shift+R.

## Next: aleea-terapeutica (exploration done, no file edits yet)
- Reference content: intro ("invitație către încetinire", potcoavă shape, Kneipp), "Ce este aleea terapeutică" + 4 texture segments, 2 photos, "Beneficii" list.
- Likely target: enrich `experiente.html` (currently thin, stock photos).
- Remaining after that: experiente-in-gradina, obiective-turistice, contact-rezervari* (4 pages), utile* (4 pages).

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
