import pytest
from pydantic import ValidationError

from app.tools.envelope import ChatToolEnvelope

VALID_ENVELOPE = {
    "items": [{"reportDay": "2026-07-26", "totalRevenue": 1000000}],
    "totalCount": 1,
    "truncated": False,
    "asOf": "2026-07-26T09:15:00+07:00",
    "timezone": "Asia/Ho_Chi_Minh",
    "source": "IStatisticalReadRepository.GetDailyRevenueAsync",
    "filtersApplied": {"Loại trừ": "Đơn huỷ, đơn nháp"},
    "definition": "doanh-thu",
    "currency": "VND",
    "warnings": [],
}


def test_envelope_hop_le_duoc_chap_nhan():
    envelope = ChatToolEnvelope.model_validate(VALID_ENVELOPE)
    assert envelope.totalCount == 1
    assert envelope.truncated is False


def test_envelope_field_la_bao_loi_ro_rang():
    drifted = {**VALID_ENVELOPE, "netRevenue": 1000000}
    with pytest.raises(ValidationError):
        ChatToolEnvelope.model_validate(drifted)


def test_envelope_thieu_field_bat_buoc_bao_loi():
    incomplete = {k: v for k, v in VALID_ENVELOPE.items() if k != "asOf"}
    with pytest.raises(ValidationError):
        ChatToolEnvelope.model_validate(incomplete)
