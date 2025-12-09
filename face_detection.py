# face_detection.py
import cv2
import sys

# 1. Haar Cascade 분류기 로드
# XML 파일 경로를 프로젝트 폴더에 맞게 설정하세요.
face_cascade_path = 'haarcascade_frontalface_default.xml'
face_cascade = cv2.CascadeClassifier(face_cascade_path)

if face_cascade.empty():
    print(f"오류: Cascade 파일을 찾을 수 없습니다. 경로: {face_cascade_path}")
    sys.exit()

# 2. 이미지 파일 불러오기
image_path = "data/face.jpg" # sample.jpg.는 얼굴이 포함된 이미지로 바꿔주세요.
img = cv2.imread(image_path)

if img is None:
    print(f"오류: 이미지를 찾을 수 없습니다. 경로: {image_path}")
    sys.exit()

# 3. 객체 감지를 위해 흑백 변환 (Haar Cascade는 흑백 이미지에서 작동합니다.)
gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

# 4. 얼굴 감지 수행
# faces는 (x, y, w, h) 형태의 튜플 리스트를 반환합니다.
faces = face_cascade.detectMultiScale(
    gray,
    scaleFactor=1.1, # 이미지 크기 검색 시 사용하는 스케일 계수 (1.1은 10%씩 줄여가며 검색)
    minNeighbors=5, # 얼굴로 판단하기 위한 최소 검출 횟수
    minSize=(30,30) # 얼굴로 인식할 최소 크기
)

# 5. 감지된 얼굴 주변에 사각형 그리기
for (x, y, w, h) in faces:
    # 이미지에 사각형을 그립니다. (이미지, 시작점, 끝점, 색상(BGR), 두께)
    cv2.rectangle(img, (x,y), (x + w, y + h), (255, 0, 0), 2)

# 6. 결과 저장 (이 부분은 유지합니다.)
cv2.imwrite("data/result_face.jpg", img) 
print("얼굴 감지 결과가 data/result_face.jpg에 저장되었습니다.")

# 7. 결과 표시
# cv2.imshow('Face Detection Result', img)

# cv2.waitKey(0)
# cv2.destroyAllWindows()