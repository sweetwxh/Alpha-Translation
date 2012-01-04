CFLAGS=-std=c99 -pedantic -Wall

all: cpk_replace

cpk_replace: cpk_replace.o cpk_file.o utf_tab.o util.o

cpk_replace.o: cpk_replace.c utf_tab.h error_stuff.h util.h cpk_file.h

cpk_file.o: cpk_file.c error_stuff.h util.h utf_tab.h

utf_tab.o: utf_tab.c utf_tab.h error_stuff.h util.h

util.o: util.c error_stuff.h util.h

clean:
	rm -f *.o cpk_replace
