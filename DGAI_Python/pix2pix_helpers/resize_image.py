from PIL import Image
from PIL import ImageOps
import os

def scale_up(folder, factor=2):
    """
    This method scales up the images in the folder by the given factor
    and then crops to the original size, expected to be 256x256
    """
    for (dirpath, dirnames, filenames) in os.walk(folder):
        for filename in filenames:
            if '.jpg' in filename:
                filepath = os.path.join(dirpath, filename)
                im = Image.open(filepath)
                (width, height) = im.size

                width = width * factor
                height = height * factor

                left = int(width / factor * 2)
                top = int(height / factor * 2)
                right = left + 256
                bottom = top + 256

                im = im.resize((width, height), resample=Image.NEAREST)
                im = im.crop((left, top, right, bottom))

                im.save(filepath)


def scale_down(folder, factor=2):
    """
    This method scales up the images in the folder by the given factor
    and then crops to the original size, expected to be 256x256
    """
    for (dirpath, dirnames, filenames) in os.walk(folder):
        for filename in filenames:
            if '.jpg' in filename:
                filepath = os.path.join(dirpath, filename)
                im = Image.open(filepath)
                (width, height) = im.size

                width = int(width / factor)
                height = int(height / factor)

                im = im.resize((width, height), resample=Image.NEAREST)
                im = ImageOps.expand(im, 64, fill=(255, 255, 255))

                im.save(filepath)
