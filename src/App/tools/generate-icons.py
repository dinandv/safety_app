"""Generates the PWA icons.

Original artwork, drawn here rather than taken from anywhere: a
high-visibility vest, which is what the four teams actually recognise
each other by on site. It is a placeholder for the Twemoji-derived logo
described in NOTICE, and deliberately not a copy of it.

The script is the source of the shape. Run it after changing anything
here; do not hand-edit the generated files.

    python src/App/tools/generate-icons.py
"""

from pathlib import Path

from PIL import Image, ImageDraw

INK = (26, 23, 20, 255)  # --warm-900
VEST = (252, 251, 101, 255)  # --brand-yellow
STRIPE = (235, 116, 33, 255)  # --brand-orange

SIZES = (72, 96, 128, 144, 152, 192, 384, 512)

# Everything is drawn on a 512 grid and scaled down. The vest stays
# inside the middle 60% so a maskable icon can be cropped to a circle
# without losing a shoulder.
GRID = 512

VEST_OUTLINE = [
    (150, 175),
    (205, 145),
    (256, 210),
    (307, 145),
    (362, 175),
    (378, 382),
    (134, 382),
]

STRIPES = [(150, 268, 362, 296), (150, 316, 362, 344)]

# The opening down the front, cut back to the background colour.
FRONT_GAP = (248, 205, 264, 382)


def draw(size: int) -> Image.Image:
    scale = 4  # supersample, then resize down for clean edges
    canvas = Image.new("RGBA", (GRID * scale, GRID * scale), INK)
    pen = ImageDraw.Draw(canvas)

    def s(points):
        return [(x * scale, y * scale) for x, y in points]

    def box(rect):
        x0, y0, x1, y1 = rect
        return [x0 * scale, y0 * scale, x1 * scale, y1 * scale]

    pen.polygon(s(VEST_OUTLINE), fill=VEST)
    for stripe in STRIPES:
        pen.rectangle(box(stripe), fill=STRIPE)
    pen.rectangle(box(FRONT_GAP), fill=INK)

    return canvas.resize((size, size), Image.LANCZOS)


def main() -> None:
    target = Path(__file__).resolve().parent.parent / "public" / "icons"
    target.mkdir(parents=True, exist_ok=True)

    for size in SIZES:
        draw(size).save(target / f"icon-{size}x{size}.png")

    favicon = Path(__file__).resolve().parent.parent / "public" / "favicon.ico"
    draw(64).save(favicon, sizes=[(16, 16), (32, 32), (48, 48), (64, 64)])

    print(f"Wrote {len(SIZES)} icons and a favicon to {target}")


if __name__ == "__main__":
    main()
