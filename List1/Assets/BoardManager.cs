using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public int playerPoints;
    public Text playerScore;
    public int computerPoints;
    public Text computerScore;

    public GameObject buttonPrefab;
    public Transform boardParent;
    public int boardSize = 15;
    public int max;
    public InputField sizeInput;
    public Text sizeException;
    private int[,] boardState;
    public Button button;
    public float cellSize;
    int tempDifficulty;
    int difficulty = 2;
    bool playerStart = true;
    private int movex;
    private int movey;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button playerButton;
    public Button computerButton;
    public GameObject easyGreen;
    public GameObject mediumGreen;
    public GameObject hardGreen;
    public GameObject playerPurple;
    public GameObject computerPurple;
    public GameObject WinnerPanel;
    public Text WinnerText;

    private int marked = 0;

    private int a; //when something is checked;

    int winner = 0;

    private int maxPlayer;
    private List<Vector2Int> playerCandidates = new List<Vector2Int>();
    Vector2Int candidate;
    private int maxComputer;
    private List<Vector2Int> computerCandidates = new List<Vector2Int>();

    private List<GameObject> buttons = new List<GameObject>();

    private void Start()
    {
        easyButton.GetComponent<Outline>().enabled = difficulty == 1 ? true: false;
        mediumButton.GetComponent<Outline>().enabled = difficulty == 2 ? true : false;
        hardButton.GetComponent<Outline>().enabled = difficulty == 3 ? true : false;

        playerButton.GetComponent<Outline>().enabled = playerStart;
        computerButton.GetComponent<Outline>().enabled = !playerStart;

        easyGreen.SetActive(false);
        mediumGreen.SetActive(false);
        hardGreen.SetActive(false);

        playerPurple.SetActive(false);
        computerPurple.SetActive(false);

        WinnerPanel.SetActive(false);

        playerPoints = PlayerPrefs.GetInt("PlayerPoints", 0);
        playerScore.GetComponent<Text>().text = playerPoints.ToString();
        computerPoints = PlayerPrefs.GetInt("ComputerPoints", 0);
        computerScore.GetComponent<Text>().text = computerPoints.ToString();
    }

    public void ResetPoints()
    {
        playerPoints = 0;
        computerPoints = 0;
        playerScore.GetComponent<Text>().text = "0";
        computerScore.GetComponent<Text>().text = "0";
        SaveScores();
    }

    public void SaveScores()
    {
        PlayerPrefs.SetInt("PlayerPoints", playerPoints);
        PlayerPrefs.SetInt("ComputerPoints", computerPoints);
        PlayerPrefs.Save();
    }
    public void SetDifficulty(int difficulty)
    {
        this.tempDifficulty = difficulty;
        if (difficulty == 1)
        {
            easyButton.GetComponent<Outline>().enabled = true;
            mediumButton.GetComponent<Outline>().enabled = false;
            hardButton.GetComponent<Outline>().enabled = false;
        }
        else if (difficulty == 2)
        {

            easyButton.GetComponent<Outline>().enabled = false;
            mediumButton.GetComponent<Outline>().enabled = true;
            hardButton.GetComponent<Outline>().enabled = false;
        }
        else if (difficulty == 3)
        {

            easyButton.GetComponent<Outline>().enabled = false;
            mediumButton.GetComponent<Outline>().enabled = false;
            hardButton.GetComponent<Outline>().enabled = true;
        }
        else
        {
            easyButton.GetComponent<Outline>().enabled = false;
            mediumButton.GetComponent<Outline>().enabled = false;
            hardButton.GetComponent<Outline>().enabled = false;
        }
    }

    public void SetPlayerStart(bool playerStart)
    {
        this.playerStart = playerStart;
        if (playerStart)
        {
            playerButton.GetComponent<Outline>().enabled = true;
            computerButton.GetComponent<Outline>().enabled = false;
        }
        else
        {
            playerButton.GetComponent<Outline>().enabled = false;
            computerButton.GetComponent<Outline>().enabled = true;
        }
    }

    public void GenerateBoard(int size)
    {
        this.difficulty = tempDifficulty;
        if (difficulty == 1)
        {
            easyGreen.SetActive(true);
            mediumGreen.SetActive(false);
            hardGreen.SetActive(false);
        }
        else if (difficulty == 2)
        {
            easyGreen.SetActive(false);
            mediumGreen.SetActive(true);
            hardGreen.SetActive(false);
        }
        else if (difficulty == 3)
        {
            easyGreen.SetActive(false);
            mediumGreen.SetActive(false);
            hardGreen.SetActive(true);
        }
        if (playerStart)
        {
            playerPurple.SetActive(true);
            computerPurple.SetActive(false);
        }
        else
        {
            playerPurple.SetActive(false);
            computerPurple.SetActive(true);
        }
        foreach (GameObject btn in buttons)
        {
            Destroy(btn);
        }
        WinnerPanel.SetActive(false);
        buttons.Clear();
        boardState = new int[size, size];
        max = size * size;
        marked = 0;

        GridLayoutGroup grid = boardParent.GetComponent<GridLayoutGroup>();
        grid.constraintCount = size;

        RectTransform boardRect = boardParent.GetComponent<RectTransform>();
        cellSize = boardRect.rect.width / size - 2;

        grid.cellSize = new Vector2(cellSize, cellSize);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                GameObject newButton = Instantiate(buttonPrefab, boardParent);
                buttons.Add(newButton);

                newButton.name = $"GridButton ({i}, {j})";

                GridButton gridButtonScript = newButton.GetComponent<GridButton>();
                if (gridButtonScript != null)
                {
                    gridButtonScript.SetBoardManagerReference(this);
                }

               // newButton.GetComponent<Button>().onClick.AddListener(OnGridButtonClick);

            }
        } 

        if (!playerStart)
        {
            ComputerMove();
        }
    }

    public void UpdateBoardSize(string input)
    {
        int newSize;
        if (int.TryParse(sizeInput.text, out newSize) && newSize >= 10 && newSize <= 20)
        {
            boardSize = newSize;
            GenerateBoard(boardSize);
            sizeException.text = "";
        }
        else
        {
            sizeException.text = "Please, type a number between 10 and 20.";
        }
    }

    public void OnGridButtonClick()
    {
        GameObject clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (clickedButton == null)
            return;

        int index = buttons.IndexOf(clickedButton);
        if (index == -1)
            return;

        int x = index / boardSize;
        int y = index % boardSize;


        if (boardState[x, y] == 0)
        {
            boardState[x, y] = 1;

            Text buttonText = clickedButton.GetComponentInChildren<Text>();

            buttonText.text = "X";
            buttonText.color = Color.red;
            buttonText.fontSize = (int)cellSize;
            clickedButton.GetComponent<Button>().interactable = false;
        }
        winner = CheckWinner();
        if (winner == 1)
        {
            foreach (GameObject btn in buttons)
            {
                btn.GetComponent<Button>().interactable = false;
            }
            WinnerPanel.SetActive(true);
            WinnerText.text = "Congratulations! \n You won!";
            playerPoints += 1;
            playerScore.GetComponent<Text>().text = playerPoints.ToString();
            playerStart = false;
            playerButton.GetComponent<Outline>().enabled = false;
            SaveScores();
            return;
        }
        marked++;
        if (marked < max)
        {
            ComputerMove();
        
        winner = CheckWinner();
         if (winner == -1)
        {
            foreach (GameObject btn in buttons)
            {
                btn.GetComponent<Button>().interactable = false;
            }
            WinnerPanel.SetActive(true);
            WinnerText.text = "Thanks for playing! \n Computer won!";
            computerPoints += 1;
            computerScore.GetComponent<Text>().text = computerPoints.ToString();
            playerStart = true;
            playerButton.GetComponent<Outline>().enabled = true;
            SaveScores();
            return;
        }
        }
        else
        {
            WinnerPanel.SetActive(true);
            WinnerText.text = "It's a draw!";
            SaveScores();
        }
    }

    public void CheckForLongestPlayerSequence()
    {
        maxPlayer = 0;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (boardState[i, j] == 1)
                {
                    a = 1;
                    while (j + a < boardSize && boardState[i, j + a] == 1)
                    {
                        a++;
                    }
                    if (a >= maxPlayer && (!(j == 0 || boardState[i, j - 1] == -1) || !(j + a - 1 == boardSize || boardState[i, j + a - 1] == -1)))
                    {
                        if (a > maxPlayer)
                        {
                            maxPlayer = a;
                            playerCandidates.Clear();
                        }
                        if (j > 0 && boardState[i, j - 1] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i, j - 1));
                        }
                        if (j + a < boardSize && boardState[i, j + a] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i, j + a));
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && boardState[i + a, j] == 1)
                    {
                        a++;
                    }
                    if (a >= maxPlayer && (!(i == 0 || boardState[i - 1, j] == -1) || !(i + a - 1 == boardSize || boardState[i + a - 1, j] == -1)))
                    {
                        if (a > maxPlayer)
                        {
                            maxPlayer = a;
                            playerCandidates.Clear();
                        }
                        if (i > 0 && boardState[i - 1, j] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i - 1, j));
                        }
                        if (i + a < boardSize && boardState[i + a, j] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i + a, j));
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && j + a < boardSize && boardState[i + a, j + a] == 1)
                    {
                        a++;
                    }
                    if (a >= maxPlayer && (!(i == 0 || j == 0 || boardState[i - 1, j - 1] == -1) || !(i + a - 1 == boardSize || j + a - 1 == boardSize || boardState[i + a - 1, j + a - 1] == -1)))
                    {
                        if (a > maxPlayer)
                        {
                            maxPlayer = a;
                            playerCandidates.Clear();
                        }
                        if (i > 0 && j > 0 && boardState[i - 1, j - 1] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i - 1, j - 1));
                        }
                        if (i + a < boardSize - 1 && j + a < boardSize - 1 && boardState[i + a, j + a] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i + a, j + a));
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && j - a >= 0 && boardState[i + a, j - a] == 1)
                    {
                        a++;
                    }
                    if (a >= maxPlayer && (!(i == 0 || j == boardSize - 1 || boardState[i - 1, j + 1] == -1) || !(i + a - 1 == boardSize || j - a + 1 == 0 || boardState[i + a - 1, j - a + 1] == -1)))
                    {
                        if (a > maxPlayer)
                        {
                            maxPlayer = a;
                            playerCandidates.Clear();
                        }
                        if (i > 0 && j < boardSize - 1 && boardState[i - 1, j + 1] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i - 1, j + 1));
                        }
                        if (i + a < boardSize - 1 && j - a >= 0 && boardState[i + a, j - a] == 0)
                        {
                            playerCandidates.Add(new Vector2Int(i + a, j - a));
                        }
                    }
                }
            }
        }
    }

    public Vector2Int CheckForFourPlayerSequence()
    {
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (boardState[i, j] == 1)
                {
                    a = 1;
                    while (j + a < boardSize && boardState[i, j + a] == 1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (j == 0)
                        {
                            if (boardState[i, j + 4] == 0)
                            {
                                return new Vector2Int(i, j + 4);
                            }
                        }
                        else if (j + 4 == boardSize - 1)
                        {
                            if (boardState[i, j - 1] == 0)
                            {
                                return new Vector2Int(i, j - 1);
                            }
                        }
                        else
                        {
                            a = Random.Range(0, 2);
                            if (a == 0)
                            {
                                if (boardState[i, j + 4] == 0)
                                {
                                    return new Vector2Int(i, j + 4);
                                }
                            } else
                            {
                                if (boardState[i, j - 1] == 0)
                                {
                                    return new Vector2Int(i, j - 1);
                                }
                            }
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && boardState[i + a, j] == 1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (i == 0)
                        {
                            if (boardState[i + 4, j] == 0)
                            {
                                return new Vector2Int(i + 4, j);
                            }
                        }
                        else if (i + 4 == boardSize - 1)
                        {
                            if (boardState[i - 1, j] == 0)
                            {
                                return new Vector2Int(i - 1, j);
                            }
                        }
                        else
                        {
                            a = Random.Range(0, 2);
                            if (a == 0)
                            {
                                if (boardState[i + 4, j] == 0)
                                {
                                    return new Vector2Int(i + 4, j);
                                }
                            }
                            else
                            {
                                if (boardState[i - 1, j] == 0)
                                {
                                    return new Vector2Int(i - 1, j);
                                }
                            }
                        }
                    }
                    a = 1;
                    while (i + a < boardSize - 1 && j + a < boardSize && boardState[i + a, j + a] == 1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (!(i == 0 || j == 0) || !(i + 4 == boardSize - 1 || j + 4 == boardSize - 1))
                        {
                            if (i == 0 || j == 0)
                            {
                                if (boardState[i + 4, j + 4] == 0)
                                {
                                    return new Vector2Int(i + 4, j + 4);
                                }
                            }
                            else if (i + 4 == boardSize - 1 || j + 4 == boardSize - 1)
                            {
                                if (boardState[i - 1, j - 1] == 0)
                                {
                                    return new Vector2Int(i - 1, j - 1);
                                }
                            }
                            else
                            {
                                a = Random.Range(0, 2);
                                if (a == 0)
                                {
                                    if (boardState[i + 4, j + 4] == 0)
                                    {
                                        return new Vector2Int(i + 4, j + 4);
                                    }
                                }
                                else
                                {
                                    if (boardState[i - 1, j - 1] == 0)
                                    {
                                        return new Vector2Int(i - 1, j - 1);
                                    }
                                }
                            }
                        }
                    }
                    a = 1;
                    while (i + a < boardSize - 1 && j - a >= 0 && boardState[i + a, j - a] == 1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (!(i == 0 || j == boardSize - 1) || !(i + 4 == boardSize - 1 || j - 4 == 0))
                        {
                            if (i == 0 || j == boardSize - 1)
                            {
                                if (boardState[i + 4, j - 4] == 0)
                                {
                                    return new Vector2Int(i + 4, j - 4);
                                }
                            }
                            else if (i + 4 == boardSize - 1 || j - 4 == 0)
                            {
                                if (boardState[i - 1, j + 1] == 0)
                                {
                                    return new Vector2Int(i - 1, j + 1);
                                }
                            }
                            else
                            {
                                a = Random.Range(0, 2);
                                if (a == 0)
                                {
                                    if (boardState[i + 4, j - 4] == 0)
                                    {
                                        return new Vector2Int(i + 4, j - 4);
                                    }
                                }
                                else
                                {
                                    if (boardState[i - 1, j + 1] == 0)
                                    {
                                        return new Vector2Int(i - 1, j + 1);
                                    }
                                }
                            }
                        }
                    }
                }
                }
        }
        return new Vector2Int(-1, -1);
    }

    public void CheckForLongestComputerSequence()
    {
        maxComputer = 0;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (boardState[i, j] == -1)
                {
                    a = 1;
                    while (j + a < boardSize && boardState[i, j + a] == -1)
                    {
                        a++;
                    }
                    if (a >= maxComputer && (!(j == 0 || boardState[i, j - 1] == 1) || !(j + a - 1 == boardSize || boardState[i, j + a - 1] == 1)))
                    {
                        if (a > maxComputer)
                        {
                            maxComputer = a;
                            computerCandidates.Clear();
                        }
                        if (j > 0 && boardState[i, j - 1] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i, j - 1));
                        }
                        if (j + a < boardSize && boardState[i, j + a] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i, j + a));
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && boardState[i + a, j] == -1)
                    {
                        a++;
                    }
                    if (a >= maxComputer && (!(i == 0 || boardState[i - 1, j] == 1) || !(i + a - 1 == boardSize || boardState[i + a - 1, j] == 1)))
                    {
                        if (a > maxComputer)
                        {
                            maxComputer = a;
                            computerCandidates.Clear();
                        }
                        if (i > 0 && boardState[i - 1, j] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i - 1, j));
                        }
                        if (i + a < boardSize && boardState[i + a, j] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i + a, j));
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && j + a < boardSize && boardState[i + a, j + a] == -1)
                    {
                        a++;
                    }
                    if (a >= maxComputer && (!(i == 0 || j == 0 || boardState[i - 1, j - 1] == 1) || !(i + a - 1 == boardSize || j + a - 1 == boardSize || boardState[i + a - 1, j + a - 1] == 1)))
                    {
                        if (a > maxComputer)
                        {
                            maxComputer = a;
                            computerCandidates.Clear();
                        }
                        if (i > 0 && j > 0 && boardState[i - 1, j - 1] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i - 1, j - 1));
                        }
                        if (i + a < boardSize && j + a < boardSize && boardState[i + a, j + a] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i + a, j + a));
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && j - a >= 0 && boardState[i + a, j - a] == -1)
                    {
                        a++;
                    }
                    if (a >= maxComputer && (!(i == 0 || j == boardSize - 1 || boardState[i - 1, j + 1] == 1) || !(i + a - 1 == boardSize || j - a + 1 == 0 || boardState[i + a - 1, j - a + 1] == 1)))
                    {
                        if (a > maxComputer)
                        {
                            maxComputer = a;
                            computerCandidates.Clear();
                        }
                        if (i > 0 && j < boardSize - 1 && boardState[i - 1, j + 1] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i - 1, j + 1));
                        }
                        if (i + a < boardSize - 1 && j - a >= 0 && boardState[i + a, j - a] == 0)
                        {
                            computerCandidates.Add(new Vector2Int(i + a, j - a));
                        }
                    }
                }
            }
        }
    }

    public Vector2Int CheckForFourComputerSequence()
    {
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (boardState[i, j] == -1)
                {
                    a = 1;
                    while (j + a < boardSize && boardState[i, j + a] == -1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (j == 0)
                        {
                            if (boardState[i, j + 4] == 0)
                            {
                                return new Vector2Int(i, j + 4);
                            }
                        }
                        else if (j + 4 == boardSize - 1)
                        {
                            if (boardState[i, j - 1] == 0)
                            {
                                return new Vector2Int(i, j - 1);
                            }
                        }
                        else
                        {
                            a = Random.Range(0, 2);
                            if (a == 0)
                            {
                                if (boardState[i, j + 4] == 0)
                                {
                                    return new Vector2Int(i, j + 4);
                                }
                            }
                            else
                            {
                                if (boardState[i, j - 1] == 0)
                                {
                                    return new Vector2Int(i, j - 1);
                                }
                            }
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && boardState[i + a, j] == -1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (i == 0)
                        {
                            if (boardState[i + 4, j] == 0)
                            {
                                return new Vector2Int(i + 4, j);
                            }
                        }
                        else if (i + 4 == boardSize - 1)
                        {
                            if (boardState[i - 1, j] == 0)
                            {
                                return new Vector2Int(i - 1, j);
                            }
                        }
                        else
                        {
                            a = Random.Range(0, 2);
                            if (a == 0)
                            {
                                if (boardState[i + 4, j] == 0)
                                {
                                    return new Vector2Int(i + 4, j);
                                }
                            }
                            else
                            {
                                if (boardState[i - 1, j] == 0)
                                {
                                    return new Vector2Int(i - 1, j);
                                }
                            }
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && j + a < boardSize && boardState[i + a, j + a] == -1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (!(i == 0 || j == 0) || !(i + 4 == boardSize - 1 || j + 4 == boardSize - 1))
                        {
                            if (i == 0 || j == 0)
                            {
                                if (boardState[i + 4, j + 4] == 0)
                                {
                                    return new Vector2Int(i + 4, j + 4);
                                }
                            }
                            else if (i + 4 == boardSize - 1 || j + 4 == boardSize - 1)
                            {
                                if (boardState[i - 1, j - 1] == 0)
                                {
                                    return new Vector2Int(i - 1, j - 1);
                                }
                            }
                            else
                            {
                                a = Random.Range(0, 2);
                                if (a == 0)
                                {
                                    if (boardState[i + 4, j + 4] == 0)
                                    {
                                        return new Vector2Int(i + 4, j + 4);
                                    }
                                }
                                else
                                {
                                    if (boardState[i - 1, j - 1] == 0)
                                    {
                                        return new Vector2Int(i - 1, j - 1);
                                    }
                                }
                            }
                        }
                    }
                    a = 1;
                    while (i + a < boardSize && j - a >= 0 && boardState[i + a, j - a] == -1)
                    {
                        a++;
                    }
                    if (a == 4)
                    {
                        if (!(i == 0 || j == boardSize - 1) || !(i + 4 == boardSize - 1 || j - 4 == 0))
                        {
                            if (i == 0 || j == boardSize - 1)
                            {
                                if (boardState[i + 4, j - 4] == 0)
                                {
                                    return new Vector2Int(i + 4, j - 4);
                                }
                            }
                            else if (i + 4 == boardSize - 1 || j - 4 == 0)
                            {
                                if (boardState[i - 1, j + 1] == 0)
                                {
                                    return new Vector2Int(i - 1, j + 1);
                                }
                            }
                            else
                            {
                                a = Random.Range(0, 2);
                                if (a == 0)
                                {
                                    if (boardState[i + 4, j - 4] == 0)
                                    {
                                        return new Vector2Int(i + 4, j - 4);
                                    }
                                }
                                else
                                {
                                    if (boardState[i - 1, j + 1] == 0)
                                    {
                                        return new Vector2Int(i - 1, j + 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void ComputerMove()
    {
        if (difficulty == 1)
        {
            movex = Random.Range(0, boardSize);
            movey = Random.Range(0, boardSize);
            while (boardState[movex, movey] != 0)
            {
                movex = Random.Range(0, boardSize);
                movey = Random.Range(0, boardSize);
            }
        }
        else if (difficulty == 2)
        {
            if (marked == 0)
            {
                movex = Random.Range(0, boardSize);
                movey = Random.Range(0, boardSize);
                while (boardState[movex, movey] != 0)
                {
                    movex = Random.Range(0, boardSize);
                    movey = Random.Range(0, boardSize);
                }
            }
            else
            {
                candidate = CheckForFourComputerSequence();
                if(candidate.x != -1)
                {
                    movex = candidate.x;
                    movey = candidate.y;
                }
                else {
                CheckForLongestPlayerSequence();
                if (maxPlayer == 0 || playerCandidates.Count == 0)
                {
                    movex = Random.Range(0, boardSize);
                    movey = Random.Range(0, boardSize);
                    while (boardState[movex, movey] != 0)
                    {
                        movex = Random.Range(0, boardSize);
                        movey = Random.Range(0, boardSize);
                    }
                }
                else
                {
                    candidate = playerCandidates[Random.Range(0, playerCandidates.Count)];
                    movex = candidate.x;
                    movey = candidate.y;
                }
                }
            }
        }
        else if (difficulty == 3)
        {
            if (marked == 0)
            {
                    movex = Random.Range(0, boardSize);
                    movey = Random.Range(0, boardSize);
                    while (boardState[movex, movey] != 0)
                    {
                        movex = Random.Range(0, boardSize);
                        movey = Random.Range(0, boardSize);
                    }
            }
            else { 
            CheckForLongestPlayerSequence();
            CheckForLongestComputerSequence();
                if (playerCandidates.Count == 0 && computerCandidates.Count == 0)
                {
                    movex = Random.Range(0, boardSize);
                    movey = Random.Range(0, boardSize);
                    while (boardState[movex, movey] != 0)
                    {
                        movex = Random.Range(0, boardSize);
                        movey = Random.Range(0, boardSize);
                    }
                }
                else if (maxComputer >= maxPlayer && computerCandidates.Count > 0)
                {
                    candidate = computerCandidates[Random.Range(0, computerCandidates.Count)];
                    movex = candidate.x;
                    movey = candidate.y;
                }
                else if (playerCandidates.Count > 0)
                {
                    candidate = playerCandidates[Random.Range(0, playerCandidates.Count)];
                    movex = candidate.x;
                    movey = candidate.y;
                }
            }
        }
        boardState[movex, movey] = -1;
        int buttonIndex = movex * boardSize + movey;
        GameObject button = buttons[buttonIndex];

        Text buttonText = button.GetComponentInChildren<Text>();

        buttonText.text = "O";
        buttonText.color = Color.blue;
        buttonText.fontSize = (int)cellSize;
        button.GetComponent<Button>().interactable = false;
        marked++;

        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (boardState[i, j] == 0)
                {
                    emptyCells.Add(new Vector2Int(i, j));
                }
            }
        }
    }

    public int CheckWinner()
    {
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (boardState[i,j] == 1)
                {
                    //checking in row
                   if (j <= boardSize - 5)
                   {
                        a = 1;
                        while (a < 5)
                        {
                            if (boardState[i, j + a] != 1)
                            {
                                break;
                            }
                            a++;
                        }
                        if (a == 5)
                        {
                            return 1;
                        }
                        //checking in first diagonal
                        if (i <= boardSize - 5)
                        {
                            a = 1;
                            while (a < 5)
                            {
                                if (boardState[i + a, j + a] != 1)
                                {
                                    break;
                                }
                                a++;
                            }
                            if (a == 5)
                            {
                                return 1;
                            }
                        }
                   }
                    if (i <= boardSize - 5)
                    {
                        //checking in column
                        a = 1;
                        while (a < 5)
                        {
                            if (boardState[i + a, j] != 1)
                            {
                                break;
                            }
                            a++;
                        }
                        if (a == 5)
                        {
                            return 1;
                        }
                        //checking in second diagonal
                        if (j >= 4) {
                            a = 1;
                        while (a < 5)
                        {
                            if (boardState[i + a, j - a] != 1)
                            {
                                break;
                            }
                            a++;
                        }
                        if (a == 5)
                        {
                            return 1;
                        }
                    }
                    }
                }
                else if (boardState[i, j] == -1)
                {
                    //checking in row
                    if (j <= boardSize - 5)
                    {
                        a = 1;
                        while (a < 5)
                        {
                            if (boardState[i, j + a] != -1)
                            {
                                break;
                            }
                            a++;
                        }
                        if (a == 5)
                        {
                            return -1;
                        }
                        //checking in first diagonal
                        if (i <= boardSize - 5)
                        {
                            a = 1;
                            while (a < 5)
                            {
                                if (boardState[i + a, j + a] != -1)
                                {
                                    break;
                                }
                                a++;
                            }
                            if (a == 5)
                            {
                                return -1;
                            }
                        }
                    }
                    if (i <= boardSize - 5)
                    {
                        //checking in column
                        a = 1;
                        while (a < 5)
                        {
                            if (boardState[i + a, j] != -1)
                            {
                                break;
                            }
                            a++;
                        }
                        if (a == 5)
                        {
                            return -1;
                        }
                        //checking in second diagonal
                        if (j >= 4)
                        {
                            a = 1;
                            while (a < 5)
                            {
                                if (boardState[i + a, j - a] != -1)
                                {
                                    break;
                                }
                                a++;
                            }
                            if (a == 5)
                            {
                                return -1;
                            }
                        }
                    }
                }
            }
        }
        return 0;
    }
}
