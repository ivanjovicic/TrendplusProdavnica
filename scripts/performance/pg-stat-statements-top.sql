SELECT
    queryid,
    calls,
    round(total_exec_time::numeric, 2) AS total_exec_time_ms,
    round(mean_exec_time::numeric, 2) AS mean_exec_time_ms,
    round((100 * total_exec_time / sum(total_exec_time) OVER ())::numeric, 2) AS pct_total_exec_time,
    rows,
    shared_blks_hit,
    shared_blks_read,
    temp_blks_read,
    temp_blks_written,
    left(regexp_replace(query, '\s+', ' ', 'g'), 300) AS normalized_query
FROM pg_stat_statements
WHERE query NOT ILIKE '%pg_stat_statements%'
ORDER BY total_exec_time DESC
LIMIT 25;

SELECT
    queryid,
    calls,
    round(mean_exec_time::numeric, 2) AS mean_exec_time_ms,
    round(max_exec_time::numeric, 2) AS max_exec_time_ms,
    rows,
    left(regexp_replace(query, '\s+', ' ', 'g'), 300) AS normalized_query
FROM pg_stat_statements
WHERE calls >= 5
  AND query NOT ILIKE '%pg_stat_statements%'
ORDER BY mean_exec_time DESC
LIMIT 25;
