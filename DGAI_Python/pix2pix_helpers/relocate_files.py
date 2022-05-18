import os
import shutil

def relocate_files(source, target):
    """
    Use this script to move files to a folder
    """

    source = os.path.realpath(source)
    target = os.path.realpath(target)

    if not os.path.isdir(target):
        os.makedirs(target)
    
    for (dirpath, dirnames, filenames) in os.walk(source):
        for filename in filenames:
            srcPath = os.path.join(dirpath, filename)
            toPath = os.path.join(target, filename)
            if os.path.isfile(toPath):
                print(f"File {toPath} exists! Overwriting has been avoided")
            else:
                shutil.copy(srcPath, toPath)
