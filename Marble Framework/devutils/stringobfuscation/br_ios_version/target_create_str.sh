#!/bin/sh

#  createstr_buildrule.sh
#  nscore
#
#  Created by giraffe on 2/6/12.

OUTPUT_PATH=${BUILD_ROOT}/generated_strings
python ${PROJECT_DIR}/stringobfuscation/create_str.py "${PRODUCT_NAME}/${PRODUCT_NAME}.strings" "${OUTPUT_PATH}/${PRODUCT_NAME}_strings"


