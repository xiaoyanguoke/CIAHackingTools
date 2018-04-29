import unittest
import os

import create_str

class TestStringObfuscation(unittest.TestCase):
    def testShouldGenerateHeaderAndCFilesGivenStrings(self):
        create_str.generateFiles("test.strings", "mooproj", True, "./")

        def assertIsFile(filepath):
            self.assertTrue(os.path.isfile(filepath), "Expecting {} to exist, but does not".format(filepath))

        assertIsFile("mooprojtest_strings.h")
        assertIsFile("proj_strings_init_strings.h")
        assertIsFile("mooprojtest_strings.c")
