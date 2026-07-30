import json
from datetime import datetime


def _parse_as_of(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


class RunSnapshot:
    def __init__(self, run_id: str):
        self.run_id = run_id
        self.as_of = None
        self._cache: dict[str, dict] = {}

    async def get(self, tool_name: str, args: dict, fetcher) -> dict:
        key = f"{tool_name}:{json.dumps(args, sort_keys=True, default=str)}"
        if key in self._cache:
            return self._cache[key]

        result = await fetcher()
        if self.as_of is None:
            self.as_of = result.get("asOf")
        self._cache[key] = result
        return result

    def warnings(self) -> list[str]:
        as_of_values = [r["asOf"] for r in self._cache.values() if r.get("asOf")]
        if len(as_of_values) < 2:
            return []

        parsed = [_parse_as_of(v) for v in as_of_values]
        spread_seconds = (max(parsed) - min(parsed)).total_seconds()
        if spread_seconds > 60:
            return ["Dữ liệu được lấy ở các thời điểm cách nhau hơn 1 phút"]
        return []
