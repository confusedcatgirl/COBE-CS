# Bytecode Emulation (COBE)

This project aims for developers who want to include not-hardcoded userspace apps inside their cosmos os.

Cosmos OS Bytecode Emulation, or COBE for short will simplify the process, and help with this so that it will be easier.

## How to Run

``python emulator.py \<bin-file\>``

## Roadmap

- Adding full Emulation capabilities

- Including a simple "compiler"

- Rewriting the emulator into C#

- Including Docs on how to port it to the own COSMOS OS

## Limitations and Strengths

The Compiler tries to be very dynamic as possible. Any variable name without spaces is possible, excluding escape sequences, as they might cause problems. Instructions are case insensitive, and are recommended to be always written uppercase. This language does not support classes, or single line coding, as every instruction requires it's own line.

Another limitation is the space placement. COBE does not support intendation or double spaces, maybe in the future :P

Also, the emulator has to be rewritten for each cosmos project to some extend, as each os handles certain instructions a bit different.

## TODO

- File I/O Capabilities

- Midi Loading and outputting

- Multithreading

- Loops

## Header

The Header ranges from Address 00000000 to 0000000F

| Address | Description | Size |
|---------|-------------|------|
| 0000 0000 | Mode of Program. 1 = CMD; 0 = GUI | 1 |
| 0000 0001 | Screen Width | 2 |
| 0000 0003 | Screen Height | 2 |

## List of Opcodes

| Hex  | Code | Description | Example | Binary Representation |
|------|------|-------------|---------|-----------------------|
| 0x00 | NOP | No Operation. Does nothing. | NOP | 00 |
| 0x01 | LBL | A variable like object. | LBL String hello: "Hello World!" | 01 02 68656C6C6F 00 2248656C6C6F20576F726C642122 |
| | | | Number num: 15 | 01 03 6E756D 00 000E |
| | | | File file: "test.txt" | 01 05 66696C65 00 22746573742E74787422 |
| 0x02 | MTH | Combines math with labels. First argument has to be a label. | MTH num1 + 10 | 02 06 6E756D31 00 03 000A 00 |
| 0x03 | PUT | Write a string to the screen. Add a \n manually for a new line. | PUT hello | 03 06 68656C6C6F0A 00 |
| | | | PUT "Hello World!" | 03 02 2248656C6C6F20576F726C642122 00 |
| | | | PUT 1 | 03 03 0001 00 |
| 0x04 | RKI | Read Keyboard Input into an existing label. | RKI yourname | 04 796F75726E616D650A 00 |
| 0x05 | RET | Required Keyword to terminate. | RET 0 | 05 03 0000 00 |
| 0x06 | MRK | Similar to Labels, but for jumps. | MRK marker01 | 06 6D61726B65723031 00 |
| 0x07 | JMP | Jumps to a specific marker. | JMP marker01 | 07 6D61726B65723031 00 |
| 0x08 | SSC | Sets the screen to w and h. | SSC 800 800 | 08 03 0320 00 03 0258 00 |
| 0x09 | BEP | Beeps in a specific length and frequency. | BEP 1000 1000 | 09 03 03E8 00 03 03E8 00 |
| 0x0A | IFJ | When the If is true, it will go down, otherwise jump to the marker. | IFJ 10 >= num1 marker01 | 0A 12 03 000A 00 01 6E756D31 00 6D61726B65723031 00 |
| | | | IFJ num1 < 19 marker1 | 0A 11 06 6E756D31 00 03 0013 00 6D61726B65723031 00 |
| 0x0B | DTB | Draws a pixel to the buffer at a specific position. | DTB 300 250 #0495AB | 0B 03 021C 00 03 00FA 00 04 0495AB 00 |
| 0x0C | CDB | Clears the draw buffer. | CDB #000000 | 0C 04 000000 00 |
| 0x0D | RFB | Removes a pixel from the draw buffer. | RFB 301 250 | 0D 03 021D 00 03 00FA 00 |
| 0x0E | WFT | Waits a specific time in milliseconds. | WFT 1000 | 0E 03 03E8 00 |

## Creating Markers

Markers are used to jump back to specific positions inside the code. There are samples in the table above, but in code they can be used like this:

```
MRK loop
PUT "A loop!"
JMP loop
```

Markers can also be used as equivalent of functions

## Templates

### Hello World

```
Header: terminal 800 600

PUT "Hello World!"
```

### Name I/O

```
Header: terminal 800 600

RKI yourname
PUT "Hello "
PUT yourname
PUT ".\nThat's a great name!"
```

## Drawing to the Screen

In this language, drawing to the screen works a bit different. Instead of drawing shapes to the screen, you adds pixels to a buffer, which is then drawn to the screen. The buffer can be modified through DTB, CDB, and RFB. The buffer does not change by itself, and has to be manually manipulated and cleared.
Drawing will only work if the Program has been defined as "graphical" in the header.

## Types

Only required when when coding directly in bytecode. Defines what type the following argument in question is.

| Hex  | Definition | Bytes used | Examples |
|------|------------|------------|----------|
| 0x01 | Boolean | 1 | 0, 1 |
| 0x02 | String | String Length | "Hello!" |
| 0x03 | Number | 2 | 12, 594 |
| 0x04 | Color | 3 | #439FA3 |
| 0x05 | File | File Size | -- |
| 0x06 | Label | -- | -- |

The label cannot be used as type, otherwise it will cause problems.

## Operators

The Common Comparisons (>, <, >=, <=, !=, ==) can be used in Ifs and certain loops. Please refer to the tables above and below for further information.

| Operator | Binary |
|----------|--------|
| + | 01 |
| - | 02 |
| * | 03 |
| / | 04 |
| > | 10 |
| < | 11 |
| >= | 12 |
| <= | 13 |
| != | 14 |
| == | 15 |