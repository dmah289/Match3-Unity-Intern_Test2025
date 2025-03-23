# Solution

## Task 1: Re-skin
1. **Phân tích**: Phép biến đổi đã cho không có quy luật đặc biệt nào → Áp dụng phép biến đổi cơ bản nhất **Cộng ma trận**
2. **Triển khai**:
    - Bước 1: Mã hóa 2 bảng skin dưới dạng số nguyên
        - Mã hóa bảng thứ nhất theo thứ tự của enum eNormalType trong file [NormalItem.cs](Assets/Scripts/Board/NormalItem.cs#L7).
        - Đọc ma trận thứ 2 từ góc trên bên trái để mã hóa và lưu thứ tự vào enum eFishType trong file [FishItem.cs](Assets/Scripts/Board/FishItem.cs#L3) 
    - Bước 2: Tính ma trận chuyển đổi bằng cách tính hiệu 2 ma trận và lưu vào mảng 2 chiều trong file [Board.cs - L41](Assets/Scripts/Board/Board.cs#L41)
    - Bước 3: Phương thức Transform duyệt qua ma trận và thực hiện biến đổi ([Board.cs](Assets/Scripts/Board/Board.cs#L757))
        - Với mỗi phần tử sử dụng thuật toán mã hóa Ceaser để đảm bảo giá trị sau biến đổi luôn nằm trong enum eNormalType.
    - Bước 4: Nhấn Space để chạy phương thức Transform [BoardController.cs - L82](Assets/Scripts/Controllers/BoardController.cs#L82)

## Task 2: Change the Gameplay

1. **Phân tích**:
    - Cần phân biệt giữa những cell ở trên Board và Bottom Cell → Đề xuất tạo 1 lớp cha Cell ([Cell.cs](Assets/Scripts/Board/Cell.cs)). Tạo 2 lớp con BoardCell và WaitingCell kế thừa lớp Cell
        - BoardCell ([BoardCell.cs](Assets/Scripts/Board/BoardCell.cs)) lưu vị trí hàng và cột trên Board
        - Các ô ở đáy chỉ nằm trên 1 hàng → lưu Index ([WaitingCell.cs](Assets/Scripts/BottomQueue/WaitingCell.cs#L10))
2. **Triển khai**:
    - Bước 1: Tạo lớp WaitingCellQueue ([WaitingCellQueue.cs](Assets/Scripts/BottomQueue/WaitingCellQueue.cs))
        - Quản lý danh sách các WaitingCell tương tự cơ chế hàng đợi ([WaitingCellQueue.cs - L8](Assets/Scripts/BottomQueue/WaitingCellQueue.cs#L8))
        - GameObject cho lớp này được khởi tạo khi bắt đầu màn chơi của tất cả LevelMode ([GameManager.cs - L94](Assets/Scripts/Controllers/GameManager.cs#L94)) đồng thời tạo các Prefab của WaitingCell và gán vào mảng để quản lý
    - Bước 3: Điều chỉnh logic chọn BoardCell bằng Raycast
        - Nếu bắt được raycast và không thuộc chế độ chơi Auto nào (Task 3) → Di chuyển Item từ BoardCell đến WaitingCell và giải phóng Item cho BoardCell
    - Bước 4: Di chuyển Item xuống hàng đợi
        - Nếu WaitingCellQueue đang thực hiện hành động nào đó -> Đợi ([WaitingCellQueue.cs - L59](Assets/Scripts/BottomQueue/WaitingCellQueue.cs#L59))
        - Nếu hàng đợi còn chỗ → Tìm phần tử có kiểu Item giống với Item được chọn ([WaitingCellQueue.cs - L98](Assets/Scripts/BottomQueue/WaitingCellQueue.cs#L98))
            - Nếu không có → chèn vào cuối hàng đợi
            - Nếu có → Chèn vào vị trí của phần tử đầu tiên tìm được
    - Bước 4: Kiểm tra nếu xuất hiện 3 phần tử giống nhau để thì dọn khỏi hàng đợi và dịch các phần tử đằng sau lên (nếu có) lấp đầy vị trí các phần tử bị dọn
    - Bước 5: Sau khi dọn kiểm tra hàng đợi vẫn đầy → Game Over (Chế độ Moves)
    - Bước 6: Nếu chưa thua → Kiểm tra số lượng phần tử trên Board, nếu đã hết → Win
        - Số lượng phần tử được giảm đi mỗi khi có Item được chọn và di chuyển xuống hàng đợi [BoardController.cs - L306](Assets/Scripts/Controllers/BoardController.cs#L306)

## Task 2.1: Requirements
- Số lượng phần tử chia hết cho 3 ([Board.cs - L136](Assets/Scripts/Board/Board.cs#L136))
    - Số lượng kiểu = tổng số phần tử trên Board / 3 (Có thể trùng nhau)
    - Lấy đủ tất cả eNormalType vào mảng (Task 3.1) và lấy random cho phần còn lại ([Board.cs - L176](Assets/Scripts/Board/Board.cs#L176))
    - Tạo 1 mảng lưu 3 lần tất cả chỉ số của các eNormalType đã lấy được và xáo trộn chúng lên ([Board.cs - L162](Assets/Scripts/Board/Board.cs#L162))
    - Gán eNormalType vào BoardCell bằng cách dựa vào index mảng 2 chiều để tính index trong mảng 1 chiều các chỉ số.

## Task 2.5 + 2.6: Requirements
1. Phân tích: 
    - Các chức năng tự động cần thông tin của cả Board và WaitingCellQueue → Kích hoạt Coroutine ngay sau khi LoadLevel tại GameManager ([GameManager.cs - L90](Assets/Scripts/Controllers/GameManager.cs#L90))
    - Việc di chuyển Item xuống hàng đợi tương đương việc sử dụng Raycast để chọn Item
2. **Triển khai AutoPlay** ([GameManager.cs - L167](Assets/Scripts/Controllers/GameManager.cs#L167))
    - Chừng nào còn phần tử trên Board thì còn duyệt qua các phần tử trên board
    - Đối với mỗi phần tử BoardCell, kiểm tra nếu có Item → Kiểm tra trạng thái hàng đợi
        - Nếu không có phần tử thì thêm luôn Item được chọn vào hàng đợi
        - Nếu có phần tử thì kiểm tra xem liệu có Item nào giống kiểu Item đang được chọn không
            - Nếu có → Thêm Item đang được chọn vào hàng đợi.
            - Nếu không → Bỏ qua Item của BoardCell đang xét.
3. **Triển khai AutoLose** ([GameManager.cs - L208](Assets/Scripts/Controllers/GameManager.cs#L208))
    - Chỉ duyệt qua các phần tử của Board 1 lần
    - Đối với mỗi phần tử BoardCell, kiểm tra nếu có Item → kiểm tra trạng thái hàng đợi
        - Nếu không có phần tử thì thêm luôn Item được chọn vào hàng đợi
        - Nếu có phần tử thì kiểm tra xem liệu có Item nào giống kiểu Item đang được chọn không
            - Nếu có nhiều nhất 1 → Thêm Item đang được chọn vào hàng đợi.
            - Nếu nhiều hơn 1 → Bỏ qua vì nếu thêm Item vào sẽ bị dọn và tạo ô trống cho hàng đợi.

## Task 3.3: Improve the gameplay
- Khi chọn Item trên BoardCell sẽ không đặt trạng thái GameOver khi WaitingCellQueue đầy([WaitingCellQueue.cs - L67](Assets/Scripts/BottomQueue/WaitingCellQueue.cs#L67))
- Muốn di chuyển Item từ WaitingCellQueue về lại Board → Lưu lại vị trí của Item khi còn trên Board khi di chuyển xuống WaitingCellQueue ([WaitingCellQueue.cs - L78](Assets/Scripts/BottomQueue/WaitingCellQueue.cs#L78))
    - Thêm sự kiện OnMouseDown vào WaitingCell (Nếu đang trong Mode Timer thì cho phép thực hiện di chuyển trở về Board) ([WaitingCell.cs - L18](Assets/Scripts/BottomQueue/WaitingCell.cs#L18))
        - Gán lại Item của WaitingCell đang được chọn vào BoardCell với vị trí (x,y) được lưu trong Item ([WaitingCell.cs - L30](Assets/Scripts/BottomQueue/WaitingCell.cs#L30))
        - Dồn các phần tử đằng sau (nếu có) của WaitingCell được chọn lên vi trí trống 
        - Cần 1 biến cờ kiểm soát liệu WaitingCellQueue có đang thực hiện hành động nào không để đợi hành động đó hoàn thành, tránh trường hợp đang dọn các phần tử giống nhau thì user click vào WaitingCell để trả lại về Board mà Item tại WaitingCell đó bị null.

