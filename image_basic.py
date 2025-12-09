# image_basic.py
import cv2
import sys

# 1. 이미지 파일 불러오기
# 파일 경로가 잘못되면 img는 None이 됩니다.
image_path = "sample.jpg"
img = cv2.imread(image_path)

if img is None:
    print(f"오류 : 이미지를 찾을 수 없습니다. 경로 : {image_path}")
    sys.exit()

# 2. 이미지 속성 출력
# (높이, 너비, 채널 수)
print(f"이미지 크기: {img.shape}")

# 3. 새 창에 이미지 표시
cv2.imshow("Original Image", img)

# 4. 창이 닫힐 때까지 대기
# 0은 무한대기를 의미합니다. 키보드 입력이 있으면 창을 닫습니다.
cv2.waitKey(0)

# 5. 모든 창 닫기
cv2.destroyAllWindows()

# 6. 이미지 파일 저장 (선택 사항)
cv2.imwrite("saved_image.png", img)
print("이미지 저장 완료: saved_image.png")