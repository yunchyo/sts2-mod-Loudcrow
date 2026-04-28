import json
import re
from pathlib import Path

from deep_translator import GoogleTranslator


ROOT = Path(__file__).resolve().parents[1]
LOC_ROOT = ROOT / "loudcrow_localization"
OVERRIDE_ROOT = ROOT / "tools" / "localization_overrides"
ENG_CARDS = LOC_ROOT / "eng" / "cards.json"
KOR_CARDS = LOC_ROOT / "kor" / "cards.json"
CARD_OVERRIDES = OVERRIDE_ROOT / "cards_kor_overrides.json"


def u(text: str) -> str:
    return text.encode("latin1").decode("unicode_escape")


TRANSLATOR = GoogleTranslator(source="en", target="ko")

TOKEN_MAP = [
    ("[color=gold][b]Magic Bullet[/b][/color]", "ZZMAGICBULLETCOLORZZ", u(r"[color=gold][b]\ub9c8\ud0c4[/b][/color]")),
    ("[color=gold][b]Bullets[/b][/color]", "ZZBULLETSCOLORZZ", u(r"[color=gold][b]\ud0c4\ud658[/b][/color]")),
    ("[color=gold][b]Bullet[/b][/color]", "ZZBULLETCOLORZZ", u(r"[color=gold][b]\ud0c4\ud658[/b][/color]")),
    ("[color=gold][b]Block[/b][/color]", "ZZBLOCKCOLORZZ", u(r"[color=gold][b]\ubc29\uc5b4\ub3c4[/b][/color]")),
    ("[color=gold][b]Strength[/b][/color]", "ZZSTRENGTHCOLORZZ", u(r"[color=gold][b]\ud798[/b][/color]")),
    ("[color=gold][b]Dexterity[/b][/color]", "ZZDEXTERITYCOLORZZ", u(r"[color=gold][b]\ubbfc\ucca9[/b][/color]")),
    ("[color=gold][b]Heat[/b][/color]", "ZZHEATCOLORZZ", u(r"[color=gold][b]\uc5f4\uae30[/b][/color]")),
    ("[color=gold][b]Vigor[/b][/color]", "ZZVIGORCOLORZZ", u(r"[color=gold][b]\ud65c\ub825[/b][/color]")),
    ("Exhaust pile", "ZZEXHAUSTPILEZZ", u(r"\uc18c\uba78\ub41c \uce74\ub4dc \ub354\ubbf8")),
    ("draw pile", "ZZDRAWPILEZZ", u(r"\ub4dc\ub85c\uc6b0 \ub354\ubbf8")),
    ("discard pile", "ZZDISCARDPILEZZ", u(r"\ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8")),
    ("your hand", "ZZYOURHANDZZ", u(r"\uc190")),
    ("this turn", "ZZTHISTURNZZ", u(r"\uc774\ubc88 \ud134")),
    ("next turn", "ZZNEXTTURNZZ", u(r"\ub2e4\uc74c \ud134")),
    ("temporary Strength", "ZZTEMPSTRENGTHZZ", u(r"\uc784\uc2dc \ud798")),
    ("temporary Dexterity", "ZZTEMPDEXTERITYZZ", u(r"\uc784\uc2dc \ubbfc\ucca9")),
    ("Magic Bullet", "ZZMAGICBULLETZZ", u(r"\ub9c8\ud0c4")),
    ("Bullets", "ZZBULLETSZZ", u(r"\ud0c4\ud658")),
    ("Bullet", "ZZBULLETZZ", u(r"\ud0c4\ud658")),
    ("Block", "ZZBLOCKZZ", u(r"\ubc29\uc5b4\ub3c4")),
    ("Strength", "ZZSTRENGTHZZ", u(r"\ud798")),
    ("Dexterity", "ZZDEXTERITYZZ", u(r"\ubbfc\ucca9")),
    ("Vigor", "ZZVIGORZZ", u(r"\ud65c\ub825")),
    ("Heat", "ZZHEATZZ", u(r"\uc5f4\uae30")),
    ("Weak", "ZZWEAKZZ", u(r"\uc57d\ud654")),
    ("Vulnerable", "ZZVULNERABLEZZ", u(r"\ucde8\uc57d")),
    ("Frail", "ZZFRAILZZ", u(r"\uc190\uc0c1")),
    ("Ethereal", "ZZETHEREALZZ", u(r"\ud718\ubc1c\uc131")),
    ("Exhaust", "ZZEXHAUSTZZ", u(r"\uc18c\uba78")),
    ("Retain", "ZZRETAINZZ", u(r"\ubcf4\uc874")),
    ("ALL enemies", "ZZALLENEMIESZZ", u(r"\ubaa8\ub4e0 \uc801")),
    ("Attack or Skill", "ZZATTACKORSKILLZZ", u(r"\uacf5\uaca9 \ub610\ub294 \uc2a4\ud0ac")),
    ("Attack", "ZZATTACKZZ", u(r"\uacf5\uaca9")),
    ("Skill", "ZZSKILLZZ", u(r"\uc2a4\ud0ac")),
    ("Power", "ZZPOWERZZ", u(r"\ud30c\uc6cc")),
]

NORMALIZE = [
    (u(r"\ud53c\ud574\ub97c \uc785\ud799\ub2c8\ub2e4"), u(r"\ud53c\ud574\ub97c \uc90d\ub2c8\ub2e4")),
    (u(r"\ub370\ubbf8\uc9c0"), u(r"\ud53c\ud574")),
    (u(r"\uadf8\uac83\uc744 \uc190\uc5d0 \ucd94\uac00\ud558\uc2ed\uc2dc\uc624."), u(r"\uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4.")),
    (u(r"\uadf8\uac83\uc744 \uc190\uc5d0 \ucd94\uac00\ud569\ub2c8\ub2e4."), u(r"\uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4.")),
    (u(r"\uc190\uc5d0 \ucd94\uac00\ud558\uc2ed\uc2dc\uc624."), u(r"\uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4.")),
    (u(r"\uc190\uc5d0 \ucd94\uac00\ud558\uc138\uc694."), u(r"\uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4.")),
    (u(r"\uc190\uc5d0 \ucd94\uac00\ud569\ub2c8\ub2e4."), u(r"\uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4.")),
    (u(r"\ub2f9\uc2e0\uc758 \uc190"), u(r"\uc190")),
    (u(r"\ucc98\ub9ac\ud569\ub2c8\ub2e4"), u(r"\uc90d\ub2c8\ub2e4")),
    (u(r"\uc120\ud0dd\ud558\uc138\uc694."), u(r"\uc120\ud0dd\ud569\ub2c8\ub2e4.")),
    (u(r"\uc18c\uba78\ub97c \uc5bb\uc2b5\ub2c8\ub2e4"), u(r"\uc18c\uba78\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4")),
    (u(r"\ud718\ubc1c\uc131\uc744 \uc5bb\uc2b5\ub2c8\ub2e4"), u(r"\ud718\ubc1c\uc131\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4")),
    (u(r"\ubcf4\uc874\uc744 \uc5bb\uc2b5\ub2c8\ub2e4"), u(r"\ubcf4\uc874\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4")),
]

MANUAL = {
    "LOUDCROWMOD-ACE_IN_THE_HOLE.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \ubcf4\uc720\ud55c [color=gold][b]\ud0c4\ud658[/b][/color] 1\uac1c\ub2f9 \ud53c\ud574\uac00 {DamagePerBullet} \uc99d\uac00\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-AIMED_SHOT.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \uc774\ubc88 \ud134 \uacf5\uaca9\uc73c\ub85c \uc18c\ubaa8\ud55c [color=gold][b]\ud0c4\ud658[/b][/color] 1\uac1c\ub2f9 \ud53c\ud574\uac00 {ConsumedBulletDamage} \uc99d\uac00\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-AMMO_DEPOT.description": u(r"\ub9e4 \ud134 \uc2dc\uc791 \uc2dc \ubb34\uc791\uc704 [color=gold][b]\ud0c4\ud658[/b][/color] {BulletCount}\uac1c\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-AMMO_SUPPLY.description": u(r"\ubb34\uc791\uc704 [color=gold][b]\ud0c4\ud658[/b][/color] {BulletCount}\uac1c\ub97c X\ud68c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-ARREST.description": u(r"\uc57d\ud654 {Weak}\ub97c \ubd80\uc5ec\ud569\ub2c8\ub2e4. [color=gold][b]\ud798[/b][/color]\uc744 {StrengthLoss} \uac10\uc18c\uc2dc\ud0b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-BREAKTHROUGH.description": u(r"\ubc29\uc5b4\ub3c4 {Block:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4. \ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8\uc5d0 [color=gold][b]\uc5f4\uae30[/b][/color] 2\uc7a5\uc744 \ub123\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-BURST.description": u(r"\uce74\ub4dc 1\uc7a5\uc744 \ubc84\ub9bd\ub2c8\ub2e4. \ubaa8\ub4e0 \uc801\uc5d0\uac8c \ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: \uc190\uc774 \ube44\uc5b4 \uc788\uc73c\uba74 \ub300\uc2e0 \ud53c\ud574 {EmptyHandDamage:diff}\ub97c \uc90d\ub2c8\ub2e4."),
    "LOUDCROWMOD-CAW_CAW.description": u(r"\ubaa8\ub4e0 \uc801\uc5d0\uac8c \ucde8\uc57d {Vulnerable}\uc640 \uc57d\ud654 {Weak}\ub97c \ubd80\uc5ec\ud569\ub2c8\ub2e4. \ub2e4\uc74c \ud134 \ubaa8\ub4e0 \uc801\uc774 [color=gold][b]\ud798[/b][/color] {NextTurnStrength}\uc744 \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-CROW_CALL.description": u(r"\uc18c\uba78\ub41c \uce74\ub4dc \ub354\ubbf8\uc5d0\uc11c \ubb34\uc791\uc704 \uce74\ub4dc 1\uc7a5\uc744 \uc0ac\uc6a9\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-CROW_FEATHER.description": u(r"\uc784\uc2dc [color=gold][b]\ud798[/b][/color] {StrengthGain}\uc744 \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-CROW_FORM.description": u(r"\ub9e4 \ud134 \uc2dc\uc791 \uc2dc \uc18c\uba78\ub41c \uce74\ub4dc \ub354\ubbf8\uc5d0\uc11c \uce74\ub4dc 3\uc7a5 \uc911 1\uc7a5\uc744 \uc120\ud0dd\ud569\ub2c8\ub2e4. \uadf8 \uce74\ub4dc\ub294 \uc774\ubc88 \ud134 \ube44\uc6a9\uc774 0\uc774 \ub418\uace0 \uc190\uc5d0 \ub4e4\uc5b4\uc635\ub2c8\ub2e4."),
    "LOUDCROWMOD-CROW_HERALD.description": u(r"\ub4dc\ub85c\uc6b0 \ub354\ubbf8\uc5d0\uc11c \uacf5\uaca9 \ub610\ub294 \uc2a4\ud0ac \uce74\ub4dc {ChoiceCount}\uc7a5\uc744 \uc120\ud0dd\ud569\ub2c8\ub2e4. \uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4. \uc18c\uba78\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-DUCK.description": u(r"\ubc29\uc5b4\ub3c4 {Block:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: \ucd94\uac00\ub85c \ubc29\uc5b4\ub3c4 {MagicBulletBlock:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-FIRE_BREATHING_GUN.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8\uc5d0 [color=gold][b]\uc5f4\uae30[/b][/color] 2\uc7a5\uc744 \ub123\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-FULL_BURST.description": u(r"\uce74\ub4dc 1\uc7a5\uc744 \ubc84\ub9bd\ub2c8\ub2e4. \ubaa8\ub4e0 \uc801\uc5d0\uac8c \ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: \ub300\uc2e0 \ud53c\ud574 {MagicBulletDamage:diff}\ub97c \uc90d\ub2c8\ub2e4. \ub9c9\ud788\uc9c0 \uc54a\uc740 \ud53c\ud574\ub9cc\ud07c [color=gold][b]\ubc29\uc5b4\ub3c4[/b][/color]\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-GHOST_BULLET.description": u(r"\uc774 \uce74\ub4dc\uac00 \uc18c\uba78\ub41c \uce74\ub4dc \ub354\ubbf8\uc5d0 \uc788\ub294 \ub3d9\uc548 [color=gold][b]\ub9c8\ud0c4[/b][/color] \ud6a8\uacfc\uc5d0\uc11c [color=gold][b]\ud0c4\ud658[/b][/color]\uc744 \uc694\uad6c\ud558\uc9c0 \uc54a\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-HAYMAKER.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \uc790\uc2e0\uc5d0\uac8c \ucde8\uc57d 1\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-HOLDOUT.description": u(r"\ud734\ub300\ud55c [color=gold][b]\ud0c4\ud658[/b][/color] 1\uac1c\ub2f9 \ubc29\uc5b4\ub3c4 {BlockPerBullet:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-MAGIC_BULLET_MARKSMAN.description": u(r"\ub0b4 \ud134 \uc2dc\uc791 \uc2dc, [color=gold][b]\ub9c8\ud0c4[/b][/color] 1\uc744 \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-MOST_WANTED.description": u(r"\uc801\uc5d0\uac8c \ud604\uc0c1\uc218\ubc30\ub97c \ubd80\uc5ec\ud569\ub2c8\ub2e4. \uc774\ubc88 \ud134 \uadf8 \ub300\uc0c1\uc5d0\uac8c \uc801\uc6a9\ub418\ub294 [color=gold][b]\ud0c4\ud658[/b][/color] \ud6a8\uacfc\uac00 2\ubc30\uac00 \ub429\ub2c8\ub2e4. [color=gold][b]\ud0c4\ud658[/b][/color] {BulletGain}\uac1c\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-OUTLAW.description": u(r"\uc774\ubc88 \ud134 \ubaa8\ub4e0 \uce74\ub4dc\uc758 \ube44\uc6a9\uc774 1 \uac10\uc18c\ud569\ub2c8\ub2e4. \uce74\ub4dc\uc5d0 \uc18c\uba78\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4. \ud134 \uc885\ub8cc \uc2dc \uc190\ud328\ub97c \ubaa8\ub450 \uc18c\uba78\uc2dc\ud0b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-PERFECT_FINISH.description": u(r"\ud134 \uc885\ub8cc \uc2dc \uc190\uc774 \ube44\uc5b4 \uc788\uc73c\uba74 \ubc29\uc5b4\ub3c4 {Block:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-READY_TO_FIRE.description": u(r"\ubc29\uc5b4\ub3c4 {Block:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4. \ub2e4\uc74c \uc0ac\uaca9 \uce74\ub4dc\uc758 \ube44\uc6a9\uc740 0\uc785\ub2c8\ub2e4."),
    "LOUDCROWMOD-RECOVERY.description": u(r"\uc18c\uba78\ub41c \uce74\ub4dc \ub354\ubbf8\uc5d0\uc11c \uce74\ub4dc 2\uc7a5\uc744 \uc120\ud0dd\ud574 \uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4. \uadf8 \uce74\ub4dc\ub4e4\uc5d0 \ud718\ubc1c\uc131\uacfc \uc18c\uba78\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-RELOAD.description": u(r"\uc18c\uba78\ub41c \uce74\ub4dc \ub354\ubbf8\uc5d0\uc11c \ubb34\uc791\uc704 \uce74\ub4dc {CardCount}\uc7a5\uc744 \ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8\ub85c \ub3cc\ub9bd\ub2c8\ub2e4. \ubb34\uc791\uc704 [color=gold][b]\ud0c4\ud658[/b][/color] {BulletCount}\uac1c\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-RICOCHET_SHOT.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. [color=gold][b]\ud0c4\ud658[/b][/color] \ud6a8\uacfc\uac00 \ubc1c\ub3d9\ud588\ub2e4\uba74 \ubb34\uc791\uc704 \uc801\uc5d0\uac8c \ud53c\ud574 {RicochetDamage:diff}\ub97c \ud55c \ubc88 \ub354 \uc90d\ub2c8\ub2e4."),
    "LOUDCROWMOD-SCROUNGE.description": u(r"\ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8\uc5d0\uc11c \uc18c\uba78\uc774 \uc544\ub2cc \uce74\ub4dc\ub97c \ubb34\uc791\uc704\ub85c 1\uc7a5 \uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-SELF_IMMOLATION.description": u(r"\uc790\uc2e0\uc5d0\uac8c \ucde8\uc57d/\uc57d\ud654/\uc190\uc0c1 \uc911 \ubb34\uc791\uc704\ub85c 2\uac00\uc9c0\ub97c \ubd80\uc5ec\ud569\ub2c8\ub2e4. \uc5d0\ub108\uc9c0\ub97c {EnergyGain}\uc5bb\uace0, \uce74\ub4dc 2\uc7a5\uc744 \ubf51\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_COMMON_01.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: \ud55c \ubc88 \ub354 \uc2dc\uc804\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_RARE03.description": u(r"\uc190\uc758 \ub2e4\ub978 \uce74\ub4dc\ub97c \ubaa8\ub450 \uc18c\uba78\uc2dc\ud0b5\ub2c8\ub2e4. \ubaa8\ub4e0 \uc801\uc5d0\uac8c \ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: \uc774\ub807\uac8c \uc18c\uba78\uc2dc\ud0a8 \uce74\ub4dc 1\uc7a5\ub2f9 \ud53c\ud574\uac00 {DamagePerExhaustedCard} \uc99d\uac00\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_RARE_02.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \uc774 \ud134 \ub3d9\uc548 \ub2e4\ub978 \uacf5\uaca9 \uce74\ub4dc\ub97c \uc0ac\uc6a9\ud560 \ub54c\ub9c8\ub2e4, \ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8\uc5d0\uc11c \uc774 \uce74\ub4dc\ub97c \ubb34\uc791\uc704 \uc801\uc5d0\uac8c \uc0ac\uc6a9\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_RARE_03.description": u(r"\uc190\uc758 \ub2e4\ub978 \uce74\ub4dc\ub97c \ubaa8\ub450 \uc18c\uba78\uc2dc\ud0b5\ub2c8\ub2e4. \ubaa8\ub4e0 \uc801\uc5d0\uac8c \ud53c\ud574 {TotalDamage:diff}\ub97c \uc90d\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_RARE_09.description": u(r"\uc18c\uba78\ub41c \uce74\ub4dc \ub354\ubbf8\uc5d0\uc11c \ubb34\uc791\uc704 \uce74\ub4dc {ReturnCount}\uc7a5\uc744 \ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8\ub85c \ub3cc\ub9bd\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_RARE_10.description": u(r"\ub2e4\uc74c \ud134 \uc2dc\uc791 \uc2dc \uce74\ub4dc {DrawCount}\uc7a5\uc744 \ubf51\uc2b5\ub2c8\ub2e4. \uadf8 \uce74\ub4dc\ub4e4\uc5d0 \uc18c\uba78\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_01.description": u(r"\ubaa8\ub4e0 \uc801\uc5d0\uac8c \ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \ud734\ub300\ud55c [color=gold][b]\ud0c4\ud658[/b][/color] 1\uac1c\ub2f9 \ud53c\ud574\uac00 {DamagePerBullet} \uc99d\uac00\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_02.description": u(r"\ubc29\uc5b4\ub3c4 {Block:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: \ucd94\uac00\ub85c \ubc29\uc5b4\ub3c4 {MagicBulletBlock:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_03.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: \ud55c \ubc88 \ub354 \uc2dc\uc804\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL03.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c X\ud68c \uc90d\ub2c8\ub2e4. [color=gold][b]\ub9c8\ud0c4[/b][/color]: 2\ud68c \ucd94\uac00\ub85c \uacf5\uaca9\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_04.description": u(r"\ubb34\uc791\uc704 [color=gold][b]\ud0c4\ud658[/b][/color] 1\uac1c\uc640 [color=gold][b]\ub9c8\ud0c4[/b][/color] 1\uc744 \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_05.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. [color=gold][b]\uc5f4\uae30[/b][/color] 1\uc7a5 \ub2f9 \ud53c\ud574\uac00 {HeatDamageBonus} \uc99d\uac00\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_06.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \uc0ac\uc6a9 \uc2dc, \uc774 \uce74\ub4dc\uc5d0 \uc7ac\uc0ac\uc6a9 1\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_07.description": u(r"\uc790\uc2e0\uc5d0\uac8c \uc57d\ud654 1\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4. \uc5d0\ub108\uc9c0\ub97c {EnergyGain}\uc5bb\uace0, \uce74\ub4dc {DrawCount}\uc7a5\uc744 \ubf51\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_08.description": u(r"\ubc84\ub9b0 \uce74\ub4dc \ub354\ubbf8\uc5d0 [color=gold][b]\uc5f4\uae30[/b][/color] {HeatCount}\uc7a5\uc744 \ub123\uace0, \uce74\ub4dc {DrawCount}\uc7a5\uc744 \ubf51\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_09.description": u(r"\uc801\uc5d0\uac8c \uc559\uc2ec\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4. \uc774 \ud134 \ub3d9\uc548 \uadf8 \uc801\uc740 \ud558\uc218\uc778\uc5d0\uac8c \ubc1b\ub294 \ud53c\ud574\uac00 2\ubc30\uac00 \ub429\ub2c8\ub2e4."),
    "LOUDCROWMOD-SPEC_SPECIAL_10.description": u(r"\ubc29\uc5b4\ub3c4 {Block:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4. \uc774 \ud134 \ub3d9\uc548 [color=gold][b]\ud798[/b][/color] {StrengthGain}\uc744 \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-STANDOFF.description": u(r"\ud53c\ud574 {Damage:diff}\ub97c \uc90d\ub2c8\ub2e4. \uc801\uc774 \uacf5\uaca9 \uc758\ub3c4\ub97c \uac00\uc9c4 \uacbd\uc6b0 [color=gold][b]\ub9c8\ud0c4[/b][/color] 1\uc744 \uc5bb\uc2b5\ub2c8\ub2e4."),
    "LOUDCROWMOD-TABLE_SHIELD.description": u(r"\ubc29\uc5b4\ub3c4 {Block:diff}\ub97c \uc5bb\uc2b5\ub2c8\ub2e4. \uc774 \ud134 \uc548\uc5d0 \ub610 \uc0ac\uc6a9\ud560 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4."),
}


def load_json(path: Path) -> dict[str, str]:
    if not path.exists():
        return {}
    return json.loads(path.read_text(encoding="utf-8"))


def translate_description(text: str) -> str:
    placeholders = []

    def repl(match: re.Match[str]) -> str:
        idx = len(placeholders)
        token = f"ZZPLACEHOLDER{idx}ZZ"
        placeholders.append((token, match.group(0)))
        return token

    masked = re.sub(r"\{[^}]+\}", repl, text)
    restore = {}
    for src, token, target in TOKEN_MAP:
        masked = masked.replace(src, token)
        restore[token] = target

    translated = TRANSLATOR.translate(masked)

    for _, token, _ in TOKEN_MAP:
        translated = translated.replace(token, restore[token])
    for token, original in placeholders:
        translated = translated.replace(token, original)

    for src, dst in NORMALIZE:
        translated = translated.replace(src, dst)

    translated = re.sub(r"\s+", " ", translated).replace(" .", ".").strip()
    translated = translated.replace(u(r"\ubc30\uae30"), u(r"\uc18c\uba78"))
    translated = translated.replace(u(r"\uc190\uc5d0 \ub123\uc2b5\ub2c8\ub2e4. \uadf8\uac83\uc740 "), "")
    translated = translated.replace(u(r"\uc18c\uba78\ub97c \ubd80\uc5ec\ud569\ub2c8\ub2e4"), u(r"\uc18c\uba78\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4"))
    translated = translated.replace(u(r"\ud718\ubc1c\uc131\ub97c \ubd80\uc5ec\ud569\ub2c8\ub2e4"), u(r"\ud718\ubc1c\uc131\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4"))
    translated = translated.replace(u(r"\ubcf4\uc874\ub97c \ubd80\uc5ec\ud569\ub2c8\ub2e4"), u(r"\ubcf4\uc874\uc744 \ubd80\uc5ec\ud569\ub2c8\ub2e4"))
    translated = re.sub(
        r"\{([^}]+)\}\ub97c \uc90d\ub2c8\ub2e4",
        lambda m: u(r"\ud53c\ud574 ") + "{" + m.group(1) + "}" + u(r"\ub97c \uc90d\ub2c8\ub2e4"),
        translated,
    )
    translated = re.sub(
        r"\[color=gold\]\[b\](.+?)\[/b\]\[/color\]\ub97c (\d+)\uac10\uc18c\uc2dc\ud0b5\ub2c8\ub2e4",
        lambda m: f"[color=gold][b]{m.group(1)}[/b][/color]\uc744 {m.group(2)} \uac10\uc18c\uc2dc\ud0b5\ub2c8\ub2e4",
        translated,
    )
    return translated


def main() -> None:
    eng_cards = load_json(ENG_CARDS)
    current_kor = load_json(KOR_CARDS)
    existing_overrides = load_json(CARD_OVERRIDES)

    description_overrides = {}
    description_keys = sorted(key for key in eng_cards if key.endswith(".description"))
    for key in description_keys:
        description_overrides[key] = MANUAL.get(key) or translate_description(eng_cards[key])

    merged_overrides = {
        **{k: v for k, v in existing_overrides.items() if not k.endswith(".description")},
        **description_overrides,
    }

    CARD_OVERRIDES.parent.mkdir(parents=True, exist_ok=True)
    CARD_OVERRIDES.write_text(
        json.dumps(dict(sorted(merged_overrides.items())), ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )

    rebuilt_cards = dict(eng_cards)
    rebuilt_cards.update(current_kor)
    rebuilt_cards.update(merged_overrides)
    KOR_CARDS.write_text(
        json.dumps(dict(sorted(rebuilt_cards.items())), ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )

    print(f"refreshed {len(description_overrides)} card descriptions")


if __name__ == "__main__":
    main()
