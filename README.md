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

- Loops

- Ifs

- Multithreading

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
| 0x01 | LBL | Label Start. | String hello: "Hello World!" | 01 01 68656C6C6F 00 2248656C6C6F20576F726C642122 |
| | | | Number num: 15 | 01 02 6E756D 00 000E |
| 0x02 | MTH | Combines math with labels. First argument has to be a label. | MTH num1 + 10 | 02 01 6E756D31 00 00 000A 00 |
| 0x03 | PUT | Write a string to the screen. Add a \n manually for a new line. | PUT hello | 03 01 68656C6C6F0A 00 |
| | | | PUT "Hello World!" | 03 02 2248656C6C6F20576F726C642122 00 |
| | | | PUT 1 | 03 03 0001 00 |
| 0x04 | RKI | Read Keyboard Input into an existing label. | RKI yourname | 04 0E 796F75726E616D650A |
| 0x05 | RET | Required Keyword to terminate. | RET 0 | 05 00 |
| 0x06 | MRK | Similar to Labels, but for jumps. | MRK marker01 | 06 6D61726B65723031 00 |
| 0x07 | JMP | Jumps to a specific marker. | JMP marker01 | 07 6D61726B65723031 00 |
| 0x08 | SSC | Sets the screen to w and h. | SSC 800 800 | 08 320 258 |
| 0x09 | BEP | Beeps anywhere, any time. Frequency and Length modifiable. | BEP 1000 1000 | 09 03E8 03E8 |
| 0x0A | IFJ | When the If is true, it will continue, otherwise jump to a specific marker. | IFJ 10 >= num1: marker01 | 0A 000A 00 12 6E756D31 00 6D61726B65723031 |
| 0x0B | DTB | Draws a pixel to the buffer at a specific position. | DTB 300 250 #0495AB | 0B 021C 00FA 0495AB |
| 0x0C | CDB | Clears the draw buffer. | CDB | 0C |
| 0x0D | RFB | Removes a pixel from the draw buffer. | RFB 301 250 | 0D 021D 00FA |
| 0x0E | WFT | Waits a specific time in milliseconds. | WFT 1000 | 0E 03E8 |
| 0x0F | LFL | Loads a specific file into a label. | LFL text N "test.txt" |
| | | | LFL text B "binary.bin" |

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

| Hex  | Definition | Bytes used |
|------|------------|------------|
| 0x00 | Boolean | 1 |
| 0x01 | String | String Length |
| 0x02 | Number | 2 |
| 0x03 | Color | 3 |
| 0x04 | File | File Size |

## Operators

The Common Comparisons (>, <, >=, <=, !=, ==) can be used in Ifs and certain loops. Please refer to the tables above and below for further information.

| Operator | Binary |
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