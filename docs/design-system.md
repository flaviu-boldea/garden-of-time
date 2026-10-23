# Brochure design system (wwwroot/styles.css)

## Section types (reuse these, don't invent new ones)
- `.intro.section` — 2-col grid, copy + framed photo (`.intro-photo` + `.photo-note`); decorative `✳`. Alternate copy/photo order for variety (e.g. pentru-cine has photo left).
- `.experiences.section` — card grid; `.experience-grid` (3 cards, optional `card-large` first) or `.experience-grid.four-cols` (4 equal cards); cards: `.card-number`, h3, p over shaded photo. Optional `.experience-tags` chips below.
- `.testimonials.section` — `.testimonial-grid` of `<blockquote class="testimonial">` (p + cite).
- `.visit` (tarife), `.location` (cum-ajungi map), `.reservation` (CTA + `.reservation-links`), `.path-section` (dark split, aleea teaser).

## Section header pattern
```
<div class="section-index">NN <span>—</span> NAME</div>
<p class="eyebrow">...</p>
<h2>Line one<br><em>italic accent.</em></h2>
```
- section-index goes INSIDE `.intro-copy` / `.section-top` div — never absolute-positioned (user rejected left-margin placement).
- `.intro-copy .section-index` margin-bottom: 40px.
- Renumber ALL sections (NN) after inserting/removing a section.

## Spacing (user-approved tight rhythm)
- `.section` padding: 88px 12%; `.intro.section`: 48px vert, min-height 520px
- `.experiences`: 64px top / 88px bottom; `.testimonials` bottom: 88px; `.visit`: 88px
- Mobile (≤680px): `.section` 64px; experiences 56/64
- Keep min-height ≤ padding+content or padding won't take effect (min-height clamps).

## Conventions
- Alt texts must describe the actual photo; captions short, warm Romanian.
- Every grid card image: `loading="lazy"`.
- 4-card sections: `.four-cols` CSS exists (4 → 2 → 1 cols responsive).
