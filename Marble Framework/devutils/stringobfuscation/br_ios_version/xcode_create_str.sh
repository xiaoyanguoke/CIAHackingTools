#!/bin/sh
inputfile=${PROJECT_DIR}/${PROJECT}/${PROJECT}.strings
outputfile=${BUILD_ROOT}/generated_strings/${PROJECT}_strings
obfspath="${PROJECT_DIR}/../nscore/stringobfuscation"
if [ "${OBFS_PATH}" ]
then
    echo "fixing obfspath = ${OBFS_PATH}"
    obfspath="${OBFS_PATH}"
fi
#pwd
#echo "input file:   $inputfile"
#echo "outnput file: $outputfile"
cd "$obfspath"
./create_str.py $inputfile $outputfile 
if [[ ! $? == 0 ]]; then
exit 1
fi
