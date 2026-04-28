import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LOC_ROOT = ROOT / "loudcrow_localization"
OVERRIDE_ROOT = ROOT / "tools" / "localization_overrides"
TABLES = ["cards", "powers", "relics", "characters", "ancients", "potions"]


def load_json(path: Path) -> dict[str, str]:
    if not path.exists():
        return {}
    return json.loads(path.read_text(encoding="utf-8"))


def build_kor_table(table: str) -> dict[str, str]:
    eng = load_json(LOC_ROOT / "eng" / f"{table}.json")
    current_kor = load_json(LOC_ROOT / "kor" / f"{table}.json")
    override_name = f"{table}_kor_overrides.json"
    overrides = load_json(OVERRIDE_ROOT / override_name)

    # `current_kor` keeps the checked-in table, while overrides exist so
    # regeneration does not wipe card-description or other hand-tuned fixes.
    merged = dict(eng)
    merged.update(current_kor)
    merged.update(overrides)

    return dict(sorted(merged.items()))


def main() -> None:
    for table in TABLES:
        merged = build_kor_table(table)
        target = LOC_ROOT / "kor" / f"{table}.json"
        target.write_text(json.dumps(merged, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("rebuilt loudcrow_localization/kor/*.json")


if __name__ == "__main__":
    main()
