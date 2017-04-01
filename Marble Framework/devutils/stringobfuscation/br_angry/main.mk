ifndef STRING_OBFUSCATION_MK
STRING_OBFUSCATION_MK := 1

$(eval $(call clear_target_vars))
STRINGOBFUSCATION_DIR := $(THIS_DIR)
TARGET := stringobfuscation
TARGET_SOURCE_DIR := $(STRINGOBFUSCATION_DIR)
SOURCES := $(wildcard $(STRINGOBFUSCATION_DIR)/*.py)
BUILD_TYPE := COPY

include $(MAKEDEPS_DIR)/add_target.mk
GEN_STRINGS := $(stringobfuscation)/create_str.py

## to generate header/c files given a .string,
# python -tt $(GEN_STRINGS) -p [projectname] input.strings <outputpath>

_tests_resources := $(wildcard $(STRINGOBFUSCATION_DIR)/tests/*)
$(foreach _res,$(_tests_resources),$(eval \
	$(call add_file_to_build,$(_res),$(stringobfuscation_BUILD_DIR),stringobfs_tests_res,$(STRINGOBFUSCATION_DIR))\
))

runtests:: $(stringobfuscation_FILES) $(stringobfs_tests_res_FILES)
	$(_v)cd $(stringobfuscation_BUILD_DIR)/tests && \
		PYTHONPATH=$(stringobfuscation_BUILD_DIR)/$(TARGET) python -m unittest discover $(PYTEST_VERBOSE)

# functions for add_strings
strings_to_src = $(strip $(patsubst %.strings,$(_BUILD_DIR)/%_strings.c,$(notdir $1)))
strings_to_header = $(strip $(patsubst %.strings,$(_BUILD_DIR)/%_strings.h,$(notdir $1)))

endif
