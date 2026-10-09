"""Build the tabular-figures fonts from D-DIN.

D-DIN has proportional digits only (no `tnum` feature). This writes a digits-only font per weight
whose ten figures share one advance (the widest digit), outlines centred. Godot uses it as the
primary font with D-DIN as fallback, so every control gets tabular figures.

The output is a Modified Version under the SIL OFL 1.1, renamed because "D-DIN" is a Reserved
Font Name. Requires fonttools:  python3 -m pip install fonttools ;  python3 tools/make_figures.py
"""
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.subset import Subsetter, Options
from fontTools.pens.t2CharStringPen import T2CharStringPen
from fontTools.pens.transformPen import TransformPen

FONTS = Path(__file__).resolve().parent.parent / "addons" / "gabarit" / "fonts"
DIGITS = "0123456789"

for src, style in (("D-DIN.otf", "Regular"), ("D-DIN-Bold.otf", "Bold")):
    font = TTFont(FONTS / src)
    opts = Options()
    opts.name_IDs = ["*"]
    opts.layout_features = []
    sub = Subsetter(opts)
    sub.populate(unicodes=[ord(d) for d in DIGITS])
    sub.subset(font)

    cmap = font.getBestCmap()
    names = [cmap[ord(d)] for d in DIGITS]
    hmtx = font["hmtx"]
    advance = max(hmtx[n][0] for n in names)
    cff = font["CFF "].cff
    top = cff.topDictIndex[0]
    glyphs = font.getGlyphSet()
    for n in names:
        width, lsb = hmtx[n]
        shift = (advance - width) / 2
        pen = T2CharStringPen(advance, glyphs)
        glyphs[n].draw(TransformPen(pen, (1, 0, 0, 1, shift, 0)))
        cs = pen.getCharString(private=top.Private, globalSubrs=cff.GlobalSubrs)
        top.CharStrings[n] = cs
        hmtx[n] = (advance, round(lsb + shift))

    family = "Gabarit Figures"
    for rec in font["name"].names:
        if rec.nameID in (1, 16):
            rec.string = family
        elif rec.nameID in (3, 4):
            rec.string = f"{family} {style}"
        elif rec.nameID == 6:
            rec.string = f"GabaritFigures-{style}"
    cff.fontNames = [f"GabaritFigures-{style}"]
    out = FONTS / f"GabaritFigures-{style}.otf"
    font.save(out)
    print(out.name, "advance", advance)
