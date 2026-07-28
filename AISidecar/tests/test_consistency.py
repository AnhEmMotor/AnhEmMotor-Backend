from app.services.run_snapshot import RunSnapshot


async def test_run_snapshot_get_cung_tool_va_args_chi_goi_fetcher_1_lan():
    calls = []

    async def fetcher():
        calls.append(1)
        return {"asOf": "2026-07-28T09:15:02+07:00", "value": 12}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher)
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher)

    assert len(calls) == 1


async def test_run_snapshot_as_of_neo_theo_lan_doc_dau_tien():
    async def fetcher_1():
        return {"asOf": "2026-07-28T09:15:02+07:00", "value": 12}

    async def fetcher_2():
        return {"asOf": "2026-07-28T09:20:00+07:00", "value": 98}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher_1)
    await snapshot.get("get_price", {"model": "SH150i"}, fetcher_2)

    assert snapshot.as_of == "2026-07-28T09:15:02+07:00"


async def test_run_snapshot_lech_asof_qua_60s_sinh_warning():
    async def fetcher_1():
        return {"asOf": "2026-07-28T09:15:00+07:00", "value": 12}

    async def fetcher_2():
        return {"asOf": "2026-07-28T09:16:30+07:00", "value": 98}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher_1)
    await snapshot.get("get_price", {"model": "SH150i"}, fetcher_2)

    assert snapshot.warnings() == ["Dữ liệu được lấy ở các thời điểm cách nhau hơn 1 phút"]


async def test_run_snapshot_khong_lech_thi_khong_co_warning():
    async def fetcher_1():
        return {"asOf": "2026-07-28T09:15:00+07:00", "value": 12}

    async def fetcher_2():
        return {"asOf": "2026-07-28T09:15:30+07:00", "value": 98}

    snapshot = RunSnapshot("run-1")
    await snapshot.get("get_inventory", {"model": "SH150i"}, fetcher_1)
    await snapshot.get("get_price", {"model": "SH150i"}, fetcher_2)

    assert snapshot.warnings() == []
