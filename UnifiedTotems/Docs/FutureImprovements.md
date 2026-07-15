# Future Performance Improvements

- [ ] Refactor `GetNearbyScarecrowEffectsPrefix` to avoid redundant `SaveMeta()`/`SendNewMeta()` calls by caching effects or using a lazy evaluation/dirty-flag system.
- [ ] Reduce the frequency of `SaveMeta()`/`SendNewMeta()` in `TotemHandler` by batching changes per frame instead of per-application.
- [ ] Implement a cache for `UnifiedTotem` components in the current scene to avoid repeated `FindObjectsOfType<UnifiedTotem>()` calls in `EvaluateEnhancedTotemsInScene`.
