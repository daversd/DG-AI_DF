from importlib.resources import path
import os
from random import random
import shutil
from tqdm import tqdm

def create_train_test(source, test_count):
    """
    Create the test and train folders inside the source folder. 
    Test will contain the specified amount of files
    """

    trainFolder = os.path.join(source, 'train')
    if not os.path.isdir(trainFolder):
        os.makedirs(trainFolder)
    
    testFolder = os.path.join(source, 'test')
    if not os.path.isdir(testFolder):
        os.makedirs(testFolder)

    test_moved = 0

    print(f"Creating test and train folders on {source}")
    for (dirpath, _, filenames) in os.walk(source):
        for filename in tqdm(filenames):
            srcPath = os.path.join(dirpath, filename)
            
            if test_moved < test_count:
                toPath = os.path.join(testFolder, filename)
                test_moved += 1
            else :
                toPath = os.path.join(trainFolder, filename)
            
            if os.path.isfile(toPath):
                print(f"File {toPath} exists! Overwriting has been avoided")
            
            else:
                shutil.move(srcPath, toPath)
        
        break
