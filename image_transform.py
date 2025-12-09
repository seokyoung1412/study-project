# image_transform.py
import cv2
import sys

image_path = "sample.jpg"
img = cv2.imread(image_path)

if img is None:
    print(f"오류 : 이미지를 찾을 수 없습니다. 경로: {image_path}")
    sys.exit()

# 1. 흑백(Gray Scale)으로 변환
# BGR -> Gray 코드를 사용합니다.
gray_img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

# 2. HSV (Hue, Saturation, Value)로 변환
# 색상 정보를 분리하여 다루기 위해 사용됩니다.
hsv_img = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

# 3. 결과 이미지 표시
cv2.imshow("Original (BGR)", img)
cv2.imshow("Gray Scale", gray_img)
cv2.imshow("HSV", hsv_img)

# 4.  창이 닫힐 때까지 대기
cv2.waitKey(0)

# 5. 모든 창 닫기
cv2.destroyAllWindows()

# 6. 이미지 크기 조정 (Resizing)
# 원본 이미지 크기의 절반으로 줄입니다.
height, width = img.shape[:2]
resized_img = cv2.resize(img, (int(width/2), int(height/2)), interpolation=cv2.INTER_LINEAR)

cv2.imshow("Resized (Half Size)",resized_img)

# 7. 창이 닫힐 때까지 대기 및 닫기
cv2.waitKey(0)
cv2.destroyAllWindows()