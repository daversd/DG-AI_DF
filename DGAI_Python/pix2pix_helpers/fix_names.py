import os

def fix_names(source, target):
    
    """Use this script to fix the name of files in a folder. It replaces the target string with the folder's name
    Note: Backup your files before proceeding.
    """
    source = os.path.realpath(source)

    for (dirpath, dirnames, filenames) in os.walk(source):
        for filename in filenames:
            x = os.path.basename(dirpath) + '_'
            if target in filename:
                renamed = filename.replace(target, x)
                os.rename(os.path.join(dirpath, filename),
                        os.path.join(dirpath, renamed))