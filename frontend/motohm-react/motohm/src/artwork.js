// MotoHM: sayt üçün orijinal vektor illüstrasiyalar (SVG).
// Şəkil faylı və public qovluğu LAZIM DEYİL: installArtwork() onları CSS dəyişənlərinə yazır
// (--hero-art, --art-bikes, --art-parts, --art-accessories, --art-rental).

// ---------------------------------------------------------------- motosiklet
const wheel = (cx, cy) => {
  let spokes = '';
  for (let i = 0; i < 6; i += 1) {
    const a = ((i * 60 + 15) * Math.PI) / 180;
    const x1 = (cx + 9 * Math.cos(a)).toFixed(1);
    const y1 = (cy + 9 * Math.sin(a)).toFixed(1);
    const x2 = (cx + 36 * Math.cos(a)).toFixed(1);
    const y2 = (cy + 36 * Math.sin(a)).toFixed(1);
    spokes += `<line x1="${x1}" y1="${y1}" x2="${x2}" y2="${y2}" stroke="#4a4a55" stroke-width="4" stroke-linecap="round"/>`;
  }
  return `
  <circle cx="${cx}" cy="${cy}" r="58" fill="#0b0b0f" stroke="#2c2c36" stroke-width="3"/>
  <circle cx="${cx}" cy="${cy}" r="50" fill="none" stroke="#17171d" stroke-width="2"/>
  <circle cx="${cx}" cy="${cy}" r="40" fill="#14141a" stroke="#7b7b88" stroke-width="5"/>
  <circle cx="${cx}" cy="${cy}" r="27" fill="none" stroke="#a5a5b0" stroke-width="2" opacity=".55"/>
  ${spokes}
  <circle cx="${cx}" cy="${cy}" r="9" fill="#9a9aa5"/>
  <circle cx="${cx}" cy="${cy}" r="4" fill="#2a2a33"/>
  <path d="M${cx + 22} ${cy - 24} L${cx + 34} ${cy - 12} L${cx + 27} ${cy - 6} L${cx + 16} ${cy - 16} Z" fill="#e53935"/>`;
};

const DEFS = `
  <linearGradient id="body" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="#34343f"/><stop offset="0.55" stop-color="#1c1c23"/><stop offset="1" stop-color="#0f0f14"/>
  </linearGradient>
  <linearGradient id="tank" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="#ff5a52"/><stop offset="0.5" stop-color="#e53935"/><stop offset="1" stop-color="#8e1b1b"/>
  </linearGradient>
  <radialGradient id="lamp" cx="0.5" cy="0.5" r="0.5">
    <stop offset="0" stop-color="#fff3c4" stop-opacity=".95"/><stop offset="1" stop-color="#fff3c4" stop-opacity="0"/>
  </radialGradient>`;

const bike = () => `
  <ellipse cx="212" cy="248" rx="205" ry="9" fill="#000" opacity=".5"/>
  <path d="M168 206 Q108 228 36 176" fill="none" stroke="#3b3b45" stroke-width="15" stroke-linecap="round"/>
  <path d="M168 203 Q108 224 36 173" fill="none" stroke="#8c8c98" stroke-width="3" stroke-linecap="round" opacity=".7"/>
  <path d="M85 187 L178 170" stroke="#24242c" stroke-width="13" stroke-linecap="round"/>
  ${wheel(85, 187)}
  ${wheel(340, 187)}
  <path d="M308 92 L340 187" stroke="#2e2e38" stroke-width="12" stroke-linecap="round"/>
  <path d="M311 98 L341 184" stroke="#cfcfd8" stroke-width="2.5" stroke-linecap="round" opacity=".45"/>
  <path d="M150 148 L250 146 L264 188 L242 214 L166 214 L144 186 Z" fill="#1e1e26" stroke="#383843" stroke-width="3"/>
  <path d="M170 160 H238 M168 172 H246 M168 184 H248 M172 196 H242" stroke="#3a3a46" stroke-width="3" stroke-linecap="round"/>
  <path d="M36 144 Q54 120 110 112 L176 110 L198 96 Q238 64 284 84 L310 90 L334 64 L354 68 L338 104 L388 132 Q392 144 376 148 L318 144 L278 158 L150 158 L98 154 Z" fill="url(#body)" stroke="#40404c" stroke-width="2"/>
  <path d="M198 98 Q238 68 284 86 L306 92 L296 110 Q240 92 204 114 Z" fill="url(#tank)"/>
  <path d="M206 96 Q240 74 280 88" fill="none" stroke="#ffb3ad" stroke-width="3" stroke-linecap="round" opacity=".7"/>
  <path d="M60 130 Q90 116 122 114 L178 112 L180 124 L104 130 Z" fill="#0a0a0d"/>
  <path d="M44 140 Q80 128 130 128" fill="none" stroke="#e53935" stroke-width="5" stroke-linecap="round"/>
  <path d="M326 70 L348 72 L336 98 L318 94 Z" fill="#9fd8ff" opacity=".28"/>
  <path d="M304 88 L290 76 L274 78" fill="none" stroke="#15151a" stroke-width="9" stroke-linecap="round"/>
  <circle cx="374" cy="128" r="34" fill="url(#lamp)"/>
  <ellipse cx="372" cy="128" rx="10" ry="6" fill="#fff3c4"/>
  <path d="M336 118 Q356 128 380 140" fill="none" stroke="#e53935" stroke-width="4" stroke-linecap="round"/>`;

const bikeG = (x, y, s) => `<g transform="translate(${x},${y}) scale(${s})">${bike()}</g>`;

// ---------------------------------------------------------------- HERO (1920x640)
// Üfüq xətti y=392-dədir: motosiklet onun üstündə, mətn bloku ilə eyni səviyyədə dayanır.
// Üfüqdən aşağıdakı geniş yol zolağı (y 392-640) axtarış paneli üçün boş saxlanılır.
const heroSvg = () => {
  let seed = 7;
  const rnd = () => {
    seed = (seed * 16807) % 2147483647;
    return seed / 2147483647;
  };
  let stars = '';
  for (let i = 0; i < 80; i += 1) {
    const r = [1, 1, 1.5, 2][Math.floor(rnd() * 4)];
    stars += `<circle cx="${Math.floor(rnd() * 1920)}" cy="${Math.floor(rnd() * 330)}" r="${r}" fill="#fff" opacity="${(0.15 + rnd() * 0.45).toFixed(2)}"/>`;
  }
  const streaks = [[560, 520, 420, 0.5], [380, 585, 600, 0.35], [700, 640, 300, 0.55], [480, 690, 500, 0.3], [820, 470, 220, 0.35], [300, 730, 520, 0.25]]
    .map(([x, y, w, o]) => `<rect x="${x}" y="${y}" width="${w}" height="3" rx="1.5" fill="url(#streak)" opacity="${o}"/>`).join('');
  let dashes = '';
  for (let x = -40; x < 1960; x += 230) dashes += `<rect x="${x}" y="610" width="130" height="8" rx="4" fill="#2b2b35"/>`;

  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1920 640" preserveAspectRatio="xMidYMid slice">
<defs>${DEFS}
  <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="#06060a"/><stop offset="0.38" stop-color="#1b0d13"/><stop offset="0.61" stop-color="#4a1719"/><stop offset="1" stop-color="#0b0b0f"/>
  </linearGradient>
  <radialGradient id="sun" cx="0.5" cy="0.5" r="0.5">
    <stop offset="0" stop-color="#ff7a45"/><stop offset="0.6" stop-color="#e53935"/><stop offset="1" stop-color="#e53935" stop-opacity="0"/>
  </radialGradient>
  <radialGradient id="halo" cx="0.5" cy="0.5" r="0.5">
    <stop offset="0" stop-color="#ff4d3d" stop-opacity=".55"/><stop offset="1" stop-color="#ff4d3d" stop-opacity="0"/>
  </radialGradient>
  <linearGradient id="streak" x1="0" y1="0" x2="1" y2="0">
    <stop offset="0" stop-color="#ff6b5e" stop-opacity="0"/><stop offset="1" stop-color="#ff6b5e" stop-opacity=".9"/>
  </linearGradient>
  <linearGradient id="ground" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="#17171d"/><stop offset="1" stop-color="#08080b"/>
  </linearGradient>
</defs>
<rect width="1920" height="640" fill="url(#sky)"/>
${stars}
<g transform="translate(0,-348)">
  <circle cx="1390" cy="560" r="620" fill="url(#halo)"/>
  <circle cx="1390" cy="560" r="230" fill="url(#sun)"/>
  <path d="M0 740 L0 640 L140 560 L260 640 L400 520 L560 650 L700 590 L860 680 L1000 600 L1180 700 L1320 620 L1500 690 L1660 600 L1800 670 L1920 620 L1920 740 Z" fill="#150b10"/>
  <path d="M0 740 L0 690 L180 630 L330 700 L520 640 L700 710 L900 660 L1100 720 L1300 670 L1500 725 L1700 665 L1920 710 L1920 740 Z" fill="#0d080c"/>
  ${streaks}
</g>
<rect x="0" y="392" width="1920" height="248" fill="url(#ground)"/>
<rect x="0" y="390" width="1920" height="3" fill="#e53935" opacity=".85"/>
<rect x="0" y="393" width="1920" height="26" fill="#e53935" opacity=".12"/>
${dashes}
<ellipse cx="1370" cy="394" rx="470" ry="20" fill="#e53935" opacity=".16"/>
${bikeG(1060, 0, 1.6)}
</svg>`;
};

// ---------------------------------------------------------------- KATEQORİYALAR (800x500)
const catBg = (c1, c2, glow, gx, gy) => `<defs>
  <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${c1}"/><stop offset="1" stop-color="${c2}"/></linearGradient>
  <radialGradient id="g" cx="0.5" cy="0.5" r="0.5"><stop offset="0" stop-color="${glow}" stop-opacity=".55"/><stop offset="1" stop-color="${glow}" stop-opacity="0"/></radialGradient>
  <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse"><path d="M40 0H0V40" fill="none" stroke="#ffffff" stroke-opacity=".035"/></pattern>
  <linearGradient id="metal" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#8d8d9a"/><stop offset="0.5" stop-color="#4a4a55"/><stop offset="1" stop-color="#25252d"/></linearGradient>
  <linearGradient id="metal2" x1="1" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#b8b8c4"/><stop offset="0.5" stop-color="#6c6c78"/><stop offset="1" stop-color="#34343d"/></linearGradient>
</defs>
<rect width="800" height="500" fill="url(#bg)"/><rect width="800" height="500" fill="url(#grid)"/>
<circle cx="${gx}" cy="${gy}" r="330" fill="url(#g)"/>`;

const gear = (cx, cy, R, r, n, hole, fill = 'url(#metal)') => {
  const pts = [];
  const step = (2 * Math.PI) / n;
  for (let i = 0; i < n; i += 1) {
    const a0 = step * i;
    [[a0 - step * 0.28, r], [a0 - step * 0.16, R], [a0 + step * 0.16, R], [a0 + step * 0.28, r]].forEach(([a, rad]) => {
      pts.push(`${(cx + rad * Math.cos(a)).toFixed(1)} ${(cy + rad * Math.sin(a)).toFixed(1)}`);
    });
  }
  return `<path d="M${pts.join(' L')} Z" fill="${fill}" stroke="#16161b" stroke-width="3" stroke-linejoin="round"/>
<circle cx="${cx}" cy="${cy}" r="${Math.round(r * 0.72)}" fill="none" stroke="#fff" stroke-opacity=".12" stroke-width="3"/>
<circle cx="${cx}" cy="${cy}" r="${hole}" fill="#14141a" stroke="#16161b" stroke-width="3"/>`;
};

const wrench = (x, y, rot) => `<g transform="translate(${x},${y}) rotate(${rot})">
  <rect x="-15" y="20" width="30" height="270" rx="15" fill="url(#metal2)" stroke="#16161b" stroke-width="3"/>
  <path d="M-34 -4 A 46 46 0 1 1 34 -4" fill="none" stroke="url(#metal2)" stroke-width="30" stroke-linecap="butt"/>
  <rect x="-34" y="-8" width="68" height="34" fill="url(#metal2)"/>
  <circle cx="0" cy="268" r="6" fill="#16161b" opacity=".6"/>
</g>`;

const svgWrap = (inner) => `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 500" preserveAspectRatio="xMidYMid slice">${inner}</svg>`;

const catBikes = () => svgWrap(`
${catBg('#1a0d12', '#3a1416', '#ff4d3d', 470, 200).replace('<defs>', `<defs>${DEFS}`)}
<rect x="0" y="332" width="800" height="168" fill="#0a0a0e" opacity=".7"/>
<rect x="0" y="331" width="800" height="3" fill="#e53935" opacity=".85"/>
${bikeG(165, 58, 1.12)}`);

const catParts = () => svgWrap(`
${catBg('#16120d', '#3a2210', '#ff9a3c', 470, 190)}
${gear(500, 205, 135, 108, 14, 44)}
${gear(330, 120, 72, 57, 10, 22, 'url(#metal2)')}
${gear(240, 235, 46, 36, 8, 14)}
${wrench(668, 150, 38)}
<circle cx="500" cy="205" r="14" fill="#e53935"/>`);

const catAccessories = () => svgWrap(`
${catBg('#190e17', '#3b1426', '#ff4d7a', 470, 200)}
<defs>
  <linearGradient id="shell" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#ff5a52"/><stop offset="0.55" stop-color="#e53935"/><stop offset="1" stop-color="#7c1a1c"/></linearGradient>
  <linearGradient id="visor" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#3b4a63"/><stop offset="0.5" stop-color="#0d1017"/><stop offset="1" stop-color="#1c2230"/></linearGradient>
</defs>
<g transform="translate(105,5)">
  <path d="M210 330 C170 270 180 150 275 105 C375 60 520 75 555 185 L562 255 C564 292 545 308 520 312 L470 322 L430 338 C410 350 395 352 380 350 L300 352 C260 352 235 350 210 330 Z" fill="url(#shell)" stroke="#5e1214" stroke-width="3"/>
  <path d="M280 108 C380 66 505 80 540 170" fill="none" stroke="#ffd0cc" stroke-width="5" stroke-linecap="round" opacity=".55"/>
  <path d="M345 214 C395 186 510 182 550 222 L553 262 C500 252 410 254 346 278 Z" fill="url(#visor)" stroke="#0a0b10" stroke-width="3"/>
  <path d="M375 214 C420 198 480 196 515 206" fill="none" stroke="#9fc4ff" stroke-width="4" stroke-linecap="round" opacity=".55"/>
  <path d="M478 292 L552 280 L548 300 C530 308 505 312 478 316 Z" fill="#0f0f14" stroke="#16161b" stroke-width="2"/>
  <path d="M470 322 L430 338 C410 350 395 352 380 350 L350 350 L350 296 L470 296 Z" fill="#1b1b22" stroke="#33333d" stroke-width="2.5"/>
  <path d="M368 300 H462 M368 312 H462 M368 324 H455" stroke="#3a3a46" stroke-width="3" stroke-linecap="round"/>
  <path d="M215 250 C240 195 300 160 345 150" fill="none" stroke="#0a0a0d" stroke-width="22" stroke-linecap="round" opacity=".55"/>
  <circle cx="300" cy="260" r="16" fill="#14141a" stroke="#34343e" stroke-width="3"/>
</g>`);

const catRental = () => {
  const teeth = [[175, 34], [203, 22], [231, 38]]
    .map(([x, h]) => `<rect x="${x}" y="18" width="14" height="${h}" fill="url(#gold)" stroke="#16161b" stroke-width="2"/>`).join('');
  return svgWrap(`
${catBg('#0d1519', '#10303a', '#2bc3d6', 520, 210)}
<defs>
  <linearGradient id="pin" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#ff5a52"/><stop offset="1" stop-color="#b3201f"/></linearGradient>
  <linearGradient id="gold" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#ffe08a"/><stop offset="0.5" stop-color="#e8a93a"/><stop offset="1" stop-color="#9a6410"/></linearGradient>
  ${DEFS}
</defs>
<path d="M-20 470 C 150 360 260 430 400 330 S 560 300 560 300" fill="none" stroke="#ffffff" stroke-opacity=".28" stroke-width="7" stroke-dasharray="26 22" stroke-linecap="round"/>
<ellipse cx="560" cy="318" rx="110" ry="24" fill="#000" opacity=".45"/>
<path d="M560 318 C 490 250 440 205 440 150 C 440 85 494 40 560 40 C 626 40 680 85 680 150 C 680 205 630 250 560 318 Z" fill="url(#pin)" stroke="#6b1213" stroke-width="3"/>
<circle cx="560" cy="150" r="82" fill="#0f1116" stroke="#2a2a33" stroke-width="4"/>
<g transform="translate(496,112) scale(0.31)">${bike()}</g>
<g transform="translate(110,165)">
  <circle cx="60" cy="60" r="62" fill="none" stroke="url(#gold)" stroke-width="26"/>
  <circle cx="60" cy="60" r="12" fill="#10303a"/>
  <rect x="120" y="44" width="170" height="32" rx="6" fill="url(#gold)" stroke="#5c3a08" stroke-width="3"/>
  <g transform="translate(0,54)">${teeth}</g>
</g>`);
};

// ---------------------------------------------------------------- CSS dəyişənlərinə yaz
const toUrl = (svg) => `url("data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}")`;

export const artworks = {
  hero: heroSvg,
  bikes: catBikes,
  parts: catParts,
  accessories: catAccessories,
  rental: catRental,
};

export function installArtwork() {
  const style = document.documentElement.style;
  style.setProperty('--hero-art', toUrl(heroSvg()));
  style.setProperty('--art-bikes', toUrl(catBikes()));
  style.setProperty('--art-parts', toUrl(catParts()));
  style.setProperty('--art-accessories', toUrl(catAccessories()));
  style.setProperty('--art-rental', toUrl(catRental()));
}