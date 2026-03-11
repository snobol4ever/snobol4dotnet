# Makefile -- run corpus SNOBOL4 programs from this repo
# Delegates to corpus/run/sno.mk for CSNOBOL4 and SPITBOL targets.

CORPUS_ROOT := $(abspath $(dir $(lastword $(MAKEFILE_LIST)))../SNOBOL4-corpus)
include $(CORPUS_ROOT)/run/sno.mk

# Convenience: run beauty.sno under both compilers
.PHONY: beauty beauty-csnobol4 beauty-spitbol

BEAUTY := $(CORPUS_ROOT)/programs/sno/beauty.sno

beauty-csnobol4:
	$(MAKE) sno-csnobol4 SNO=$(BEAUTY)

beauty-spitbol:
	$(MAKE) sno-spitbol SNO=$(BEAUTY)

beauty: beauty-csnobol4
