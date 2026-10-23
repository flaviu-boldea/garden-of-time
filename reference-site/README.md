# Grădina Timpului — local static copy

Static snapshot of https://sites.google.com/view/gradinatimpului-laboldici/acasa.
The original Romanian content, page layout, images and fonts are downloaded locally.
Google's application scripts and analytics are removed; a small local navigation menu
replaces the Google Sites navigation, including its nested pages and mobile menu.

## Run locally

With Python installed, run `python serve.py` from this folder and open
http://127.0.0.1:8000. Stop the server with Ctrl+C.

Alternatively use VS Code's **Terminal → Run Task → Serve local site**.
You can also open `index.html` directly; a local HTTP server is recommended.
There is no build step, database, login, Node installation or API key.

## Offline limitations

Downloaded pages, images, styles, fonts, and navigation work offline.
Links to external websites and embedded Google Forms, maps, weather, chat, videos,
or other third-party services still require the internet. They are not converted
into local backends, and form submissions are not stored locally.
The download report lists every page, downloaded asset, external iframe and failure.
Some services may refuse to be embedded on localhost; use the adjacent external link.

## Refresh the snapshot (optional)

The already-downloaded site needs only Python's standard library to run.
To download a fresh copy, first run `python -m pip install -r requirements.txt`,
then `python mirror_site.py`. Refreshing overwrites the generated HTML and report,
so preserve any manual HTML edits first. Local CSS and JavaScript are separate files.

## Files

- `index.html` and the other HTML files: editable static pages.
- `assets/`: downloaded images, styles and fonts.
- `local.css` / `local.js`: standalone responsive navigation and compatibility fixes.
- `mirror-report.json`: source URLs, download time, assets and external embeds.
- `mirror_site.py`: repeatable crawler and static conversion.
- `serve.py`: local-only web server bound to 127.0.0.1.
- `verify_site.py`: run `python verify_site.py` to check local links and assets
	(requires the optional downloader dependencies above).