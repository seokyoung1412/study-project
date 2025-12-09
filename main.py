# main.py
import numpy as np

class DataProcessor:
    def __init__(self, data):
        self.data = data

    def get_average(self):
        #numpy를 이용해 평균 계산
        return np.mean(self.data)
    def show_info(self):
        # f-string을 이용하여 출력 형식을 지정합니다.
        print(f"입력 데이터: {self.data}")
        print(f"평균 점수: {self.get_average():.2f}")

if __name__ == "__main__":
    scores = [80, 95, 70, 100, 85]
    processor = DataProcessor(scores)
    processor.show_info()