# Working with AI agents

The port was built by AI coding agents under the maintainer's direction. This page records what made that work and what did not.

## What worked

- **Written rules and gates.** A short set of non-negotiable rules (fidelity first, evidence for every claim, no value tuning,
  record every difference) and gate files that must exist before code changes kept the agents consistent across long sessions.
- **Persistent records instead of memory.** The compatibility register, the list of disproved hypotheses, the failure ledger and
  per-topic reports carried knowledge between sessions. An agent starting a session read them first.
- **Small, verifiable steps.** Each change was followed by a build and a run that could confirm or refute it.
- **Measurement on demand.** Agents could build and run test programs on both engine versions, so engine questions were answered
  by measurement rather than recall.
- **Questions from the maintainer.** Requests such as "is this proven, with evidence?" and "the original does not do this, find a
  better answer" repeatedly exposed overclaims and premature explanations.

## What went wrong

- **Plausible explanations without evidence.** Agents produced confident causes from code reading or general engine knowledge.
  Several were wrong. The remedy was to require a measurement or an explicit "inferred" label.
- **Misreading a symptom.** An analysis attributed a death to a game mechanic that the log did not support; the actual cause (a
  timer) was visible in the code once read.
- **Tool results taken at face value.** Background tasks reported completion early, a build queued behind another never started,
  and verification tools passed planted faults. Results were trusted only after checking the processes and outputs directly.
- **Resource contention.** Running several memory-heavy jobs in parallel caused failures unrelated to the code. One heavy job at a
  time removed them.

## Recommendations

1. Require every claim to carry its evidence class: proven, supported or inferred.
2. Keep a list of disproved hypotheses and have agents read it before starting.
3. Prefer a small measurement to a long explanation.
4. Verify tool output, process state and files directly before reporting a result.
5. Keep the original build's identity (version, hashes) in every record.
