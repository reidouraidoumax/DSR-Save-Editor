# Devil Slayer Raksasi - Save Editor
A Windows Forms (.NET 10) save editor for Devil Slayer Raksasi (PS4 saves,
also works on PC saves - same file format).

<img width="1275" height="846" alt="image" src="https://github.com/user-attachments/assets/be1fee0f-8eef-4766-9ff5-301a4da30e61" />

 **Import Save** (or Ctrl+O): pick a `.dat` save file. The banner shows the
  file name, whether the on-disk checksum currently validates, your souls and
  kills. (A checksum mismatch just means another tool edited the file since
  the game last saved it. That's harmless, and it's fixed automatically on export.)
- Each category pill (Resources / Vitals / Recovery / Offense / Defense /
  Movement / Talents / Meta) shows every known field as a card: the original
  field name, its example value underneath, and the current value from your
  save. Use the box directly, the **-/+** buttons, or the Up/Down arrow keys.
- Every field is a plain number, including the former 0/1 fields like
  `ANTI_SLOW` or `BRAVE_BADGE`, which can go above 1.
- Card states:
  - **IN SAVE**: the field exists in this save.
  - **NOT SET** (dashed border): the game hasn't written this field yet.
    Type a number into it and it becomes **WILL CREATE**; it's added to the
    save on export.
  - **EDITED** (amber): you changed an existing value; "was X" shows the old one.
  - **INVALID** (red): not a plain number (`123`, `-0.5`,
    `3703.1498881687`; no letters, commas, or scientific notation).
- Leaving an existing field's box blank leaves that field untouched.
- Each pill shows how many pending edits its category has, and the footer
  shows the total. Closing or importing another save with pending edits asks first.
- **Export Save** (or Ctrl+S): validates everything, writes only the fields
  you changed or created, and **recalculates the checksum automatically**.

  ## Notes on the fields

- Every field's example value comes directly from the PC save editor
  author's own confirmed-safe list - even the "ludicrous" ones (8,000,000 HP
  etc.) are apparently fine.
- `RESURRECTION_EVIL` isn't on that list but was found in real save data
  during reverse engineering, so it's included too.
- When you set `ATTACK_RAW`, `DODGE_RAW`, `ANTI_CHARM`, `SIGHT`, or
  `BONUS_SOUL_RAW`, the editor also mirrors the value onto the character's
  `baseline` stat block *if that field already exists there* - this keeps
  buffable stats consistent instead of reverting on the next recalculation.
  It will not create new baseline entries for fields that were never there.
- Only the **player's own** attrs block is edited - the code specifically
  avoids a lookalike `attrs` table used by a minor companion/pet entity that
  only ever holds `HP`/`MAX_HP_RAW`.

  
