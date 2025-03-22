# Solution

## Task 1: Re-skin
1. **Phân tích**: Nhận thấy phép biến đổi đã cho không có quy luật đặc biệt nào → áp dụng phép biến đổi cơ bản nhất là **Cộng ma trận**
2. **Triển khai**:
    - Bước 1: Mã hóa 2 bảng skin dưới dạng số nguyên
        - Mã hóa bảng thứ nhất theo thứ tự của enum eNormalType trong file [NormalItem.cs](Assets/Scripts/Board/NormalItem.cs#L7).
        - Đọc bảng thứ 2 và đọc ma trận từ góc trên bên trái để mã hóa và lưu thứ tự vào enum eFishType trong file [FishItem.cs](Assets/Scripts/Board/FishItem.cs#L3)
    - Bước 2: Tính ma trận chuyển đổi bằng cách tính hiệu ma trận thứ 2 và ma trận thứ nhất
    - Bước 3: Lưu ma trận vào mảng 2 chiều trong file [Board.cs](Assets/Scripts/Board/Board.cs#L41)