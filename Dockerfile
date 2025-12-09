# Dokerfile

# 1. Base Image 지정: 애플리케이션의 기반이 될 OS와 Python 버전
# opencv-python을 위한 Python 3.12 Slim 버전을 사용합니다.
FROM python:3.12-slim

# 1-1. 시스템 의존성 설치 (OpenCV 구동 및 설치에 필수)
# build-essential, cmake: 복잡한 Python 패키지를 설치(빌드)하는 데 필요
# libgl1, libsm6, libxext6: OpenCV 런타임에 필요
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        build-essential \
        cmake \
        libgl1 \
        libsm6 \
        libxext6 \
        libglib2.0-0 \
        && rm -rf /var/lib/apt/lists/*

# 2. 작업 디렉토리 설정
# 컨테이너 내부에서 코드가 실행될 폴더를 /app으로 지정
WORKDIR /app

# 3. 호스트의 requirements.txt 파일을 컨테이너의 /app으로 복사
# 이 파일에 적힌 라이브러리 목록을 기반으로 의존성을 설치합니다.
COPY requirements.txt .

# 4. 의존성 설치
# requirementstxt에 나열된 모든 라이브러리(numpy, opencv-python 등 를 설치합니다.)
RUN pip install --no-cache-dir -r requirements.txt

# 5. 호스트의 애플리케이션 파일들을 컨테이너의 /app으로 복사
# 우리가 만든 Python 코드 (*.py)와 XML 파일을 복사합니다.
COPY . /app

# 6. 컨테이너가 시작될 때 실행할 명령어 지정
# 컨테이너가 실행되면 face_detection.py를 실행합니다.
CMD ["python", "face_detection.py"]