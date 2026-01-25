# oz_gm_01_proj02_ParkHOSOUNH
게임개발 1기 2차 프로젝트 Personal13 Repository

flowchart LR
    A[MainScene] -->|Start| B[StageSelectScene]
    A -->|Settings| A1[Settings UI]
    A1 -->|Close| A

    B -->|Select Stage| C[Stage Scene]
    B -->|Back| A

    C -->|ESC Pause| P[PauseMenu]
    P -->|Resume| C
    P -->|Restart| C
    P -->|ExitToStageSelect| B
    P -->|GoToMain| A

    flowchart TD
    I[User Input: ESC or Menu Button] --> P[Pause()]
    P --> T[Time.timeScale = 0]
    P --> C1[Cursor: visible + unlocked]
    P --> L1[InputLockManager.Acquire("PauseMenu")]
    P --> UI1[menuPanel.SetActive(true)]

    R[Resume Button or ESC] --> R1[Resume()]
    R1 --> UI2[menuPanel.SetActive(false)]
    R1 --> T2[Time.timeScale = 1]
    R1 --> L2[InputLockManager.Release("PauseMenu")]

    R1 --> D{isFPSScene?}
    D -->|Yes| C2[Cursor: hidden + locked]
    D -->|No| C3[Cursor: visible + unlocked]

    flowchart TD
    A[Fire Input] --> B{Can Shoot?}
    B -->|No: ammo<=0| X[Show: need reload]
    B -->|Yes| C[Animator Trigger: Attack]
    C --> D[Get bullet from BulletManager pool]
    D --> E{Bullet exists?}
    E -->|No| Y[Return]
    E -->|Yes| F[Set bullet position/rotation]
    F --> G[Bullet.Shot(dir, speed)]
    G --> H[Ammo--]
    H --> I[ScoreManager.ConsumeAmmo(...) -> HUD update]
    I --> J[Debug Log]
    J --> K{fireSound exists?}
    K -->|Yes| L[SoundManager.PlaySfx(fireSound)]
    K -->|No| M[End]

    flowchart LR
    A[Stage Clear Event] --> B[Update clear progress]
    B --> C[DataManager.Save()]
    C --> D[PlayerPrefs / persistent data written]

    E[Game Start / Scene Load] --> F[DataManager.Load()]
    F --> G[Apply: stage clear state]
    G --> H[UnlockStage / StageSelect updates UI]
    H --> I[Playable stages / unlocked weapons reflected]
