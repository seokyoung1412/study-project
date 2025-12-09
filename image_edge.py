# image_edge.py
import cv2
import sys

image_path = "sample.jpg"
img = cv2.imread(image_path)

if img is None:
    print(f"오류: 이미지를 찾을 수 없습니다. 경로: {image_path}")
    sys.exit()

# 1. 노이즈 제거 (블러링)
# 커널 크기는 홀수(5,5)로 지정하며, 클수록 더 많이 흐려집니다.
blurred_img = cv2.GaussianBlur(img ,(5,5), 0)

# 2. Canny 경계선 검출
# Canny(이미지, 최소 임곗값, 최대 임곗값)
# 보통 최소 임곗값:최대 임곗값 비율은 1:2 또는 1:3으로 설정합니다.
edges = cv2.Canny(blurred_img, 100, 200)

# 3. 결과 표시
cv2.imshow("Original", img)
cv2.imshow("Blurred", blurred_img)
cv2.imshow("Canny edges", edges)

cv2.waitKey(0)
cv2.destroyALLWindows()