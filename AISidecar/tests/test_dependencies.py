import pytest

PROTECTED_ENDPOINTS = [
    ("/manager-chat", {"session_id": "s1", "message": "xin chào"}),
    ("/manager-chat/generate-title", {"message": "xin chào"}),
]


@pytest.mark.parametrize("path,payload", PROTECTED_ENDPOINTS)
def test_thieu_internal_secret_tra_403(client, path, payload):
    resp = client.post(path, json=payload, headers={"Authorization": "Bearer fake"})
    assert resp.status_code == 403, f"{path} phải yêu cầu X-Internal-Secret"


@pytest.mark.parametrize("path,payload", PROTECTED_ENDPOINTS)
def test_sai_internal_secret_tra_403(client, path, payload):
    resp = client.post(path, json=payload, headers={
        "Authorization": "Bearer fake",
        "X-Internal-Secret": "sai-secret",
    })
    assert resp.status_code == 403


@pytest.mark.parametrize("path,payload", PROTECTED_ENDPOINTS)
def test_internal_secret_rong_tra_403(client, path, payload):
    resp = client.post(path, json=payload, headers={
        "Authorization": "Bearer fake",
        "X-Internal-Secret": "",
    })
    assert resp.status_code == 403


def test_thieu_authorization_tra_401(client, internal_secret):
    resp = client.post("/manager-chat",
                       json={"session_id": "s1", "message": "xin chào"},
                       headers={"X-Internal-Secret": internal_secret})
    assert resp.status_code == 401


def test_dung_internal_secret_thi_qua_duoc(client, internal_secret):
    resp = client.post("/manager-chat/generate-title",
                       json={"message": "doanh thu tháng này"},
                       headers={"X-Internal-Secret": internal_secret})
    assert resp.status_code == 200
    assert resp.json()["title"] == "doanh thu tháng này"


@pytest.mark.parametrize("path,payload", PROTECTED_ENDPOINTS)
def test_env_secret_vang_mat_van_chan(client, monkeypatch, path, payload):
    monkeypatch.delenv("BACKEND_INTERNAL_SECRET", raising=False)

    for headers in (
        {"Authorization": "Bearer fake"},                                
        {"Authorization": "Bearer fake", "X-Internal-Secret": ""},       
    ):
        resp = client.post(path, json=payload, headers=headers)
        assert resp.status_code == 403, f"{path} với headers={headers} phải bị chặn"


@pytest.mark.parametrize("path,payload", PROTECTED_ENDPOINTS)
def test_env_secret_rong_van_chan(client, monkeypatch, path, payload):
    monkeypatch.setenv("BACKEND_INTERNAL_SECRET", "")

    resp = client.post(path, json=payload, headers={
        "Authorization": "Bearer fake",
        "X-Internal-Secret": "",
    })
    assert resp.status_code == 403


def test_health_khong_yeu_cau_secret(client):
    resp = client.get("/")
    assert resp.status_code == 200
    body = resp.json()
    assert body["status"] == "ok"
    text = str(body).lower()
    for leak in ("secret", "api_key", "apikey", "token", "backend_url"):
        assert leak not in text
