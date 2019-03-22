﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{

    public List<Customer> customers;
    public List<Ingredient> ingredients;
    public Transform viewport;
    public OrderItem oItem;
    Order currentOrder;
    GameObject currentOrderPrefab;
    public GameObject star;
    Taste result;
    bool isPlay;
    public static int playingTime = 0;
    public static int interval = 30;
    public GameObject gOver;
    List<Order> orders=new List<Order>();

    public void StartGame()
    {
        isPlay = true;
        StartCoroutine(RecursiveCounter());
    }
    public void PauseGame()
    {
        isPlay = false;
    }
    public void EndGame()
    {
        isPlay = false;
        gOver.SetActive(true);
    }

    private void Update()
    {
        if (isPlay)
        {
            Debug.Log("current customer: "+ currentOrder.customer.customerName );
        }
    }

    IEnumerator RecursiveCounter()
    {

        if (isPlay)
        {
            if (playingTime < 300)
            {
                CreateOrder(customers[Random.Range(0, customers.Count)]);
                playingTime += interval;
                if (currentOrder.isFinished)
                    SetCustomer();
            }
            else
                EndGame();
            yield return new WaitForSeconds(interval);
            StartCoroutine(RecursiveCounter());  
        }
        else
            yield break;

    }

    void CreateOrder(Customer _customer)
    {
        Order order = new Order(_customer,ingredients,2);
        orders.Add(order);
        oItem.ClearTexts();
        for (int i = 0; i < order.finalIngredients.Count; i++)
            oItem.ingredients[i].text = order.finalIngredients[i].ingredientName;
        oItem.customerName.text = order.customer.customerName;
        order.orderPrefab = Instantiate(oItem.prefab, viewport);

        if (orders.Count == 1)
        {
            currentOrder = orders[0];
            currentOrderPrefab = currentOrder.orderPrefab;
        }
        else
        {
            orders[orders.Count - 2].nextOrder = order;
        }
    }
    int customerIndex;

    void SetCustomer()
    {
        customerIndex++;
        if (orders.Count==customerIndex+1)
        {
            currentOrder = currentOrder.nextOrder;
            currentOrderPrefab = currentOrder.orderPrefab;
        }
        else
        {
            Debug.Log("Elinde sipariş yok.");
            customerIndex--;
        }
    }
    string percentage;
    int kalan;
    public void FinishOrder()
    {
        if (!currentOrder.isFinished)
        {
            foreach (Taste taste in currentOrder.customer.Tastes)
            {
                if (taste.totalInputCount == 0)
                {
                    if (taste.isLike)
                        taste.tasteRating = -1;
                    else
                        taste.tasteRating = 1;
                }
            }
            currentOrder.customer.CalculateAverageSatisfactionValue();
            if (currentOrder.customer.averageTasteRatingnValue > 0)
            {
                percentage = currentOrder.customer.averageTasteRatingnValue.ToString("0.##").Split(',')[1];
                kalan = int.Parse(percentage) % 10;
                Debug.Log("percantage: "+percentage);
                Debug.Log("kalan: "+kalan);
                for (int i = 0; i < (int.Parse(percentage) - kalan) / 10; i++)
                    Instantiate(star, currentOrderPrefab.GetComponent<OrderItem>().satisfaction.transform);

                if (kalan != 0)
                {
                    GameObject star1 = Instantiate(star, currentOrderPrefab.GetComponent<OrderItem>().satisfaction.transform);
                    star1.GetComponent<Image>().fillAmount = (float)kalan / 10.0f;
                }
            }
            else
            {
                Debug.Log("Hiç yıldız alamadım");
            }

            foreach (Taste taste in currentOrder.customer.Tastes)
            {
                taste.tasteRating = 0;
                taste.totalInputCount = 0;
            }
            Debug.Log(currentOrder.customer.averageTasteRatingnValue);
            currentOrder.isFinished = true;
            currentOrder.customer.averageTasteRatingnValue = 0;
        }        
        SetCustomer();
        
    }

    public void AddIngredient(int ingredientID)
    {
            for (int i = 0; i < ingredients[ingredientID].tastes.Count; i++)
            {
                result = currentOrder.customer.Tastes.Where(t => t.taste == ingredients[ingredientID].tastes[i].taste).ToList().FirstOrDefault();
                if (result != null)
                {
                    result.totalInputCount += ingredients[ingredientID].tastes[i].tasteInput;
                    result.CalculateAverageTasteRating(Satisfaction.CalculateSatisfaction(result.x_max, result.x_zero, result.totalInputCount, result.isLike == true ? 1 : -1));
                }
            }
            currentOrder.customer.CalculateAverageSatisfactionValue();
    }

}


