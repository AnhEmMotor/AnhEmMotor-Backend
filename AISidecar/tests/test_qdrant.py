from app.services import qdrant_client as qc


class FakeQdrantClient:
    def __init__(self):
        self.collections = set()
        self.create_collection_calls = []

    async def collection_exists(self, name):
        return name in self.collections

    async def create_collection(self, name, vectors_config=None):
        self.create_collection_calls.append(name)
        self.collections.add(name)


def _install_fake_client(monkeypatch):
    fake = FakeQdrantClient()
    monkeypatch.setattr(qc, "get_client", lambda: fake)
    return fake


async def test_ensure_collections_tao_moi_khi_chua_co(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    await qc.ensure_collections()
    assert fake.create_collection_calls == [qc.PLAN_TEMPLATE_COLLECTION]


async def test_ensure_collections_khong_tao_lai_khi_da_co(monkeypatch):
    fake = _install_fake_client(monkeypatch)
    fake.collections.add(qc.PLAN_TEMPLATE_COLLECTION)

    await qc.ensure_collections()

    assert fake.create_collection_calls == []


def test_khong_index_du_lieu_nhay_cam():
    forbidden = {"orders", "customers", "payroll", "revenue", "debt"}
    assert not (forbidden & {c.lower() for c in qc.INDEXED_COLLECTIONS})
