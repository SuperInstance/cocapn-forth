# Makefile — CoCapn Forth
# Run tests with gforth

GFORTH = gforth

.PHONY: test clean

test: cocapn-test.fs
	$(GFORTH) cocapn-test.fs -e bye

clean:
	rm -f *.o *.s *.bc *.i *~ core
