from app.services import qdrant_client as qc


class FakeQdrantClient:
    def __init__(self):
        self.collections = set()
        self.upserted = {}
        self.deleted_ids = []
        self.last_search_filter = None
        self.search_hits = []
        self.aliases = {}
        self.create_collection_calls = []

    async def collection_exists(self, name):
        return name in self.collections

    async def create_collection(self, name, vectors_config=None):
        self.create_collection_calls.append(name)
        self.collections.add(name)
        self.upserted.setdefault(name, [])

    async def delete_collection(self, name):
        self.collections.discard(name)
        self.upserted.pop(name, None)

    async def create_payload_index(self, collection, field, schema):
        pass

    async def upsert(self, collection_name, points):
        self.upserted.setdefault(collection_name, []).append(points)

    async def delete(self, collection_name, points_selector):
        self.deleted_ids.append(points_selector.points)

    async def count(self, collection_name):
        class _Count:
            def __init__(self, count):
                self.count = count
        total = sum(len(batch) for batch in self.upserted.get(collection_name, []))
        return _Count(total)

    async def query_points(self, collection_name, query, query_filter=None, limit=8, score_threshold=None):
        self.last_search_filter = query_filter
        class _Result:
            def __init__(self, points):
                self.points = points
        hits = [h for h in self.search_hits if score_threshold is None or h.score >= score_threshold]
        return _Result(hits[:limit])

    async def update_collection_aliases(self, change_aliases_operations):
        for op in change_aliases_operations:
            if hasattr(op, "delete_alias") and op.delete_alias:
                self.aliases.pop(op.delete_alias.alias_name, None)
            if hasattr(op, "create_alias") and op.create_alias:
                self.aliases[op.create_alias.alias_name] = op.create_alias.collection_name


class FakeHit:
    def __init__(self, score, payload):
        self.score = score
        self.payload = payload


def _install_fake_client(monkeypatch):
    fake = FakeQdrantClient()
    monkeypatch.setattr(qc, "get_client", lambda: fake)
    monkeypatch.setattr(qc, "embed", _fake_embed)
    qc._embedding_cache.clear()
    return fake


async def _fake_embed(text):
    return [0.1] * qc.VECTOR_SIZE


async def test_ensure_collections_tao_moi_khi_chua_co(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    await qc.ensure_collections()
    assert set(fake.create_collection_calls) == {
        qc.PRODUCT_COLLECTION, qc.KNOWLEDGE_COLLECTION, qc.PLAN_TEMPLATE_COLLECTION,
    }


async def test_ensure_collections_khong_tao_lai_khi_da_co(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    fake.collections.add(qc.PRODUCT_COLLECTION)
    fake.collections.add(qc.KNOWLEDGE_COLLECTION)
    fake.collections.add(qc.PLAN_TEMPLATE_COLLECTION)

    await qc.ensure_collections()

    assert fake.create_collection_calls == []


async def test_ensure_collections_chi_tao_collection_con_thieu(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    fake.collections.add(qc.PRODUCT_COLLECTION)

    await qc.ensure_collections()

    assert set(fake.create_collection_calls) == {qc.KNOWLEDGE_COLLECTION, qc.PLAN_TEMPLATE_COLLECTION}


async def test_luon_loc_is_active(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    await qc.search_products("xe ga", in_stock_only=False)
    keys = {c.key for c in fake.last_search_filter.must}
    assert "is_active" in keys


async def test_score_threshold_cat_ket_qua_rac(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    fake.search_hits = [FakeHit(0.31, {"productId": 1}), FakeHit(0.28, {"productId": 2})]
    result = await qc.search_products("abcxyz không tồn tại")
    assert result == []


async def test_reindex_dung_alias_khong_ghi_de_collection_dang_phuc_vu(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    items = [{"productId": str(i), "name": f"SP {i}"} for i in range(3)]
    result = await qc.reindex_products(items, expected_count=3)
    assert result["aliasSwitched"] is True
    assert fake.aliases[qc.PRODUCT_COLLECTION] == f"{qc.PRODUCT_COLLECTION}_v2"


async def test_reindex_khong_doi_alias_khi_thieu_diem(monkeypatch):
    _install_fake_client(monkeypatch)
    items = [{"productId": "1", "name": "SP 1"}]
    result = await qc.reindex_products(items, expected_count=5)
    assert result["aliasSwitched"] is False


async def test_ingest_theo_lo_toi_da_100(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    items = [{"productId": str(i), "name": f"SP {i}"} for i in range(250)]
    await qc.upsert_products(items)
    batches = fake.upserted[qc.PRODUCT_COLLECTION]
    assert all(len(batch) <= 100 for batch in batches)
    assert sum(len(batch) for batch in batches) == 250


def test_khong_index_du_lieu_nhay_cam():
    forbidden = {"orders", "customers", "payroll", "revenue", "debt"}
    assert not (forbidden & {c.lower() for c in qc.INDEXED_COLLECTIONS})


def test_build_product_text_khong_nhet_json():
    text = qc.build_product_text({
        "name": "Honda SH 150i", "brand": "Honda",
        "colors": ["Đỏ"], "description": "x" * 2000,
    })
    assert "{" not in text and "}" not in text
    assert "Honda SH 150i" in text
    assert len(text) < 1200
