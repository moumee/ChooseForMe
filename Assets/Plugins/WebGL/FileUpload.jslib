// FileUpload.jslib
mergeInto(LibraryManager.library, {
    // 파일 선택창을 열기 위한 함수
    UploadFile: function (gameObjectName, callbackMethodName, fileType) {
        // 1. 숨겨진 <input type="file"> HTML 엘리먼트를 동적으로 생성합니다.
        var fileInput = document.createElement("input");
        fileInput.type = "file";
        fileInput.accept = Pointer_stringify(fileType); // C#에서 받은 파일 타입(예: "image/*")을 설정합니다.
        fileInput.style.display = "none"; // 화면에 보이지 않게 숨깁니다.

        // 2. 사용자가 파일을 선택하면 실행될 이벤트를 등록합니다.
        fileInput.onchange = function (event) {
            var file = event.target.files[0];
            if (!file) {
                return;
            }

            var reader = new FileReader();
            
            // 3. 파일을 Base64 데이터 URL 형태로 읽습니다.
            reader.readAsDataURL(file);
            reader.onload = function (e) {
                // 4. 읽기가 완료되면, 파일 이름과 파일 데이터를 Unity로 다시 보냅니다.
                // Unity의 SendMessage 함수를 사용하여 지정된 GameObject의 메소드를 호출합니다.
                unityInstance.SendMessage(
                    Pointer_stringify(gameObjectName), 
                    Pointer_stringify(callbackMethodName), 
                    reader.result // Base64 데이터 URL (예: "data:image/jpeg;base64,...")
                );
            };

            // 작업이 끝나면 생성했던 엘리먼트를 제거합니다.
            document.body.removeChild(fileInput);
        };

        // 준비된 fileInput 엘리먼트를 body에 추가하고, 프로그래밍적으로 클릭합니다.
        document.body.appendChild(fileInput);
        fileInput.click();
    }
});