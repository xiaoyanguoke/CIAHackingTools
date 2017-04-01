STRINGOBFUSCATION_DIR := $(THIS_DIR)

include $(MAKEDEPS_DIR)/add_target_env.mk
include $(STRINGOBFUSCATION_DIR)/main.mk
# for this file, sources are blah.string files.
# it generates intermediate .c and .h files
# and then finally produces .o

# example
# input flameskimmer.strings
# output: BUILD_DIR/flameskimmer_strings.o, BUILD_DIR/flameskimmer_strings.h
#
# intermediate: BUILD_DIR/flameskimmer_strings.c
# NOTE: only one source string file is supported at the moment. more source files is undetermined.

# given a source file and obj dir, create a obj file [name] in the objdir

_C_SOURCES := $(call strings_to_src,$(SOURCES))
_H_SOURCES := $(call strings_to_header,$(SOURCES))

# export for referencing outside here.
$(TARGET)_STRINGS_H := $(_H_SOURCES)
$(TARGET)_STRINGS_C := $(_C_SOURCES)

# TODO: maybe use our own verbose flag instead of PYTEST_FLAGS
$(_C_SOURCES): $(SOURCES) | $(_BUILD_DIR) $(stringobfuscation_FILES)
	$(_v)echo Generate strings $@
	$(_v)python -tt $(GEN_STRINGS) $(PYTEST_VERBOSE) $^ $(abspath $(dir $@))

$(_H_SOURCES): $(_C_SOURCES)

$(_TARGET_FULLPATH): $(_C_SOURCES) | $(_BUILD_DIR)
	$(_v)$(CC) $(CPPFLAGS) $(CFLAGS) -c -o $@ $^


$(eval $(call clear_target_env))
clear_target_env :=

