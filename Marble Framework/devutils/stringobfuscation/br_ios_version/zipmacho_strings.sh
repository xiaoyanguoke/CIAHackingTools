sourcedir=$1
if [ -z "${sourcedir}" ]
then
    echo "Usage: $0 <source directory>"
    exit 1
fi

if [ !  -d "${sourcedir}"  ]
then
    echo "${sourcedir} is not a directory"
    echo "Usage: $0 <source directory>"
    exit 1

fi

sourcedir_name=`basename ${sourcedir}`

outdir="/tmp/${sourcedir_name}"
stringsdir="${sourcedir_name}_strings"


function ismacho {
    path=$1
    res=`file "${path}"`
    ismach=`file "${path}" | grep "Mach-O"`
    if [ "$ismach" ]
    then
        return 1
    else
        return 0
    fi
}

mkdir -p "${outdir}"
curdir=`pwd`
cd "${sourcedir}"
mkdir "${stringsdir}"
find "${sourcedir}" -exec unzip -d "${outdir}" -o {} \; 2>> /dev/null 1>> /dev/null
find "${outdir}" | while read filepath
do
    ismacho "$filepath"
    docheck=$?
    if [ ${docheck} -eq 1 ]
    then
        name=`basename "${filepath}"`
        fullname="${name}.strings.txt"
        outpath="${stringsdir}/${fullname}"
        echo "writing strings file ${outpath}"
        strings - "${filepath}" > "${outpath}"
    fi
done
rm -rf "${outdir}"
cd "${curdir}"
