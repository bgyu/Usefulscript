fancy-git is a very good tool to beautify your terminal for git:
https://github.com/diogocavilha/fancy-git/tree/master

However, when you install it under WSL, like Fedora/Ubuntu,
after you have installed fancy-git, you found that the symbols can't show correctly in the terminal.

To solve this issue, you need to install powerline fonts under Windows,
and set the correct fonts for Windows Terminal.

The fonts can be found here:
https://github.com/diogocavilha/fancy-git/tree/master/fonts

You can just download these fonts under Windows,
and double click the fonts to install them.

As personal experience, the font works for me is "SourceCodePro+Powerline+Awesome+Regular.ttf".
The name is "'SourceCodePro+Powerline+Awesome Regular".
You can also set this font as the first font in Visual Studio Code font family,
so that terminal under VS Code can show the symbols correctly.

Important note:
You are logging to Linux use Windows Terminal or VS Code Terminal,
but it is Windows to render these symbols, not Linux.
So you have to install these fonts for Windows, not Linux.
