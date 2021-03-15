# Freecell Solver

A freecell solver implemented using [A* search algorithm](https://en.wikipedia.org/wiki/A*_search_algorithm).

## Build

Make sure you have [.net core 3.0 or later SDK](https://dotnet.microsoft.com/download) installed then run the following command in the terminal:

```
./build
```

## Run

Once built, you can solve any MS Freecell deal # with the following command:

```
./dist/release/fc-solve run -d 1
```

or run directly using dotnet run without building:

```
cd src
dotnet run -c release -- run -d 1
```

The output will be something like this:

```
Processing deal #1
Solver: A* - using 16 cores
Done in 00:00:00.0334143 - initial id: 12 - visited nodes: 2,181 - #moves: 97
moves: 4a41{1}4b4c4h40{1}64{1}76{1}a774{2}37{1}63{2}6a6h2h71{2}2d20{1}50{1}50{1}5h0h5h7h7hbh56{1}7h05{4}27{1}23{1}d21h01{1}43{3}a404{1}0h0h4h37{4}30{3}13{4}1a21{2}a220{2}c212{3}1h50{5}15{1}1a1h0h4h0h0h7ha431{5}34{1}35{1}3h0h1h7h0h1h0h1h2h2h13{2}61{2}6h7h0h3h7h0h2h3h7h0h2h3h4h02{2}0h1h2h5h1h2h4h5h
```

### Move encoding explanation:

* **01234567** represent columns indices. i.e. **40{3}**: move *top 3* cards from *tableau 4* to *tableau 0*
* **abcd** represent reserve/freecells slots. i.e. **6a**: move top card from *tableau 6* to *reserve*
* **h** represent foundation. i.e. **7h**: move top card from *tableau 7* to *foundation*
* **{n}** represent a move size of *n*

### Choosing the MS Freecell deal number to solve

Use the `-d <n>` to specify which deal number to solve. For example, to solve deal #200:

```
./dist/release/fc-solve run -d 200
```

### Visualizing solution

You can ask the solver to output solution visualization using the `-v <PATH>`, this will output an html file:

```
./dist/release/fc-solve run -d 1 -v "./path-to-html"
```

### Other options

Use `-h` to list all possible commands and options.

## Credits

The following resources and open source projects were a huge help in building this solver:

* https://www.cs.bgu.ac.il/~sipper/publications/freecell.pdf
* https://github.com/macroxue/freecell/
* https://github.com/shlomif/fc-solve
* https://github.com/heineman/algorithms-nutshell-2ed

Resources and open source projects used for visualizer:

* https://animejs.com/
* https://fontawesome.com/