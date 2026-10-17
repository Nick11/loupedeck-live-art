#!/usr/bin/env python3
"""Add icon templates for all ART plugin actions to an exported Loupedeck profile.

Every icon in icons/buttons/<id>.svg becomes ActionIcons/<action name>.ict in the profile:
the SVG as image, the automatic action text hidden (same as editing each icon by hand
in the Icon Editor and switching the text off).

Usage: tools/apply-icons-to-profile.py <exported.lp5> <output.lp5>
"""

import base64
import json
import sys
import zipfile
from pathlib import Path

ICONS = Path(__file__).resolve().parent.parent / "icons" / "buttons"
PREFIX = "$ArtRawEditor___Loupedeck.ArtRawEditorPlugin."
SHORTCUT_GROUPS = ("tab.", "history.", "view.", "navigate.", "tool.", "output.")
SLIDER_OPS = (".plus", ".minus", ".reset")


def action_name(icon_id: str) -> str:
    if icon_id == "hover.wheel":
        return f"{PREFIX}ArtHoverWheelAdjustment"
    if icon_id == "zoom.wheel":
        return f"{PREFIX}ArtZoomAdjustment"
    if icon_id.endswith(SLIDER_OPS):
        return f"{PREFIX}ArtSliderCommand___{icon_id}"
    if icon_id.startswith(SHORTCUT_GROUPS):
        return f"{PREFIX}ArtShortcutCommand___{icon_id}"
    return f"{PREFIX}ArtSliderAdjustment___{icon_id}"


def icon_template(svg: Path) -> bytes:
    template = {
        "backgroundColor": 4278190080,
        "items": [
            {
                "image": base64.b64encode(svg.read_bytes()).decode("ascii"),
                "imageFileName": svg.name,
                "imageColor": 4294967295,
                "imageRotation": "None",
                "isVisible": True,
                "itemType": "Image",
                "area": {"x": 0, "y": 0, "width": 100, "height": 100},
            },
            {
                "text": "",
                "originalText": None,
                "textColor": 4294967295,
                "fontSize": 5,
                "fontName": "Arial",
                "isVisible": False,
                "itemType": "Text",
                "area": {"x": 0, "y": 70, "width": 100, "height": 30},
            },
        ],
    }
    return json.dumps(template, indent=2).encode("utf-8")


def main(source: str, target: str) -> None:
    templates = {f"ActionIcons/{action_name(svg.stem)}.ict": icon_template(svg) for svg in sorted(ICONS.glob("*.svg"))}

    with zipfile.ZipFile(source) as zin, zipfile.ZipFile(target, "w", zipfile.ZIP_DEFLATED) as zout:
        for info in zin.infolist():
            if info.filename not in templates:
                zout.writestr(info, zin.read(info.filename))
        for name, data in templates.items():
            zout.writestr(name, data)

    print(f"{len(templates)} icon templates written to {target}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    main(sys.argv[1], sys.argv[2])
