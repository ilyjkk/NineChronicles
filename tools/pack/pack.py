import argparse
import os
import os.path
import logging
import tarfile
import zipfile
from zipfile import ZIP_DEFLATED


parser = argparse.ArgumentParser()
parser.add_argument('out_dir')
parser.add_argument('platform', choices={'Android', 'StandaloneOSX', 'StandaloneWindows', 'StandaloneLinux64'})
parser.add_argument('input_dir')
parser.add_argument(
    '--verbose', '-v',
    action='store_const', const=logging.DEBUG, default=logging.INFO,
)


def zip_for_macos(build_result_dir: str, out_dir: str):
    archive_path = os.path.join(out_dir, 'macOS.tar.gz')
    executable_path = os.path.join(
        build_result_dir,
        '9c.app/Contents/MacOS/9c'
    )
    os.chmod(executable_path, 0o755)
    with tarfile.open(archive_path, 'w:gz') as archive:
        for arcname in os.listdir(build_result_dir):
            name = os.path.join(build_result_dir, arcname)
            archive.add(name, arcname=arcname)
    logging.info('Added: %s <- %s', arcname, name)


def zip_for_linux(build_result_dir: str, out_dir: str):
    archive_path = os.path.join(out_dir, 'Linux.tar.gz')
    executable_path = os.path.join(
        build_result_dir,
        '9c'
    )
    os.chmod(executable_path, 0o755)
    with tarfile.open(archive_path, 'w:gz') as archive:
        for arcname in os.listdir(build_result_dir):
            name = os.path.join(build_result_dir, arcname)
            archive.add(name, arcname=arcname)
    logging.info('Added: %s <- %s', arcname, name)


def zip_for_windows(build_result_dir: str, out_dir: str):
    archive_path = os.path.join(out_dir, 'Windows.zip')
    with zipfile.ZipFile(archive_path, 'w', ZIP_DEFLATED) as archive:
        basepath = os.path.abspath(build_result_dir) + os.sep
        for path, dirs, files in os.walk(build_result_dir):
            logging.debug('Walk: %r, %r, %r', path, dirs, files)
            for name in files + dirs:
                fullname = os.path.abspath(os.path.join(path, name))
                assert fullname.startswith(basepath)
                relname = fullname[len(basepath):]
                archive.write(fullname, relname)
                logging.info('Added: %s <- %s', relname, fullname)


def zip_for_android(build_result_dir: str, out_dir: str):
    archive_path = os.path.join(out_dir, 'Android.zip')
    with zipfile.ZipFile(archive_path, 'w', ZIP_DEFLATED) as archive:
        basepath = os.path.abspath(build_result_dir) + os.sep
        for path, dirs, files in os.walk(build_result_dir):
            logging.debug('Walk: %r, %r, %r', path, dirs, files)
            for name in files + dirs:
                fullname = os.path.abspath(os.path.join(path, name))
                assert fullname.startswith(basepath)
                relname = fullname[len(basepath):]
                archive.write(fullname, relname)
                logging.info('Added: %s <- %s', relname, fullname)


ZIP = {
    "Android": zip_for_android,
    "StandaloneOSX": zip_for_macos,
    "StandaloneLinux64": zip_for_linux,
    "StandaloneWindows": zip_for_windows,
}


def main() -> None:
    args = parser.parse_args()
    logging.basicConfig(level=args.verbose)

    os.makedirs(args.out_dir, exist_ok=True)

    build_result_dir = os.path.join(
        args.input_dir,
        args.platform
    )

    try:
        ZIP[args.platform](build_result_dir, args.out_dir)
    except KeyError:
        raise Exception(f'unsupported platform: {args.platform}')
    logging.info("Finish")


if __name__ == '__main__':
    main()
