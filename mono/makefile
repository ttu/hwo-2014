.PHONY: clean build

all: build

clean:
	rm -rf bot.exe

build: bot.exe

bot.exe: ./src/HWO/Program.cs
	xbuild ./src/HWO/HWO.csproj