﻿using System;
using System.Collections.Generic;
using System.Linq;
 using NUnit.Framework;
 using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using ShipItTest.Builders;

namespace ShipItTest
{
    public class EmployeeControllerTests : AbstractBaseTest
    {
        EmployeeController employeeController = new EmployeeController(new EmployeeRepository());
        EmployeeRepository employeeRepository = new EmployeeRepository();

        private const string NAME = "Gissell Sadeem";
        private const int WAREHOUSE_ID = 1;

        [Test]
        public void TestRoundtripEmployeeRepository()
        {
            onSetUp();
            var employee = new EmployeeBuilder().CreateEmployee();
            employeeRepository.AddEmployees(new List<Employee>() {employee});
            Assert.AreEqual(employeeRepository.GetEmployeesByName(employee.Name).First().Name, employee.Name);
            Assert.AreEqual(employeeRepository.GetEmployeesByName(employee.Name).First().Ext, employee.ext);
            Assert.AreEqual(employeeRepository.GetEmployeesByName(employee.Name).First().WarehouseId, employee.WarehouseId);
        }

        [Test]
        public void TestGetEmployeeByName()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() {employeeBuilder.CreateEmployee()});
            var result = employeeController.GetByName(NAME);
            var found = result.Employees.First();
            var correctEmployee = employeeBuilder.CreateEmployee();
            Assert.IsTrue(found.Name == correctEmployee.Name);
            Assert.IsTrue(found.ext == correctEmployee.ext);
            Assert.IsTrue(found.role == correctEmployee.role);
            Assert.IsTrue(found.WarehouseId == correctEmployee.WarehouseId);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void TestGetEmployeeByEmployeeId()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            var createdEmployee = employeeRepository.AddEmployees(new List<Employee>() {employeeBuilder.CreateEmployee()});
            var result = employeeController.GetById(createdEmployee.First().Id);

            var correctEmployee = employeeBuilder.CreateEmployee();
            Assert.IsTrue(EmployeesAreEqual(correctEmployee, result.Employees.First()));
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void TestGetEmployeesByWarehouseId()
        {
            onSetUp();
            var employeeBuilderA = new EmployeeBuilder().setWarehouseId(WAREHOUSE_ID).setName("A");
            var employeeBuilderB = new EmployeeBuilder().setWarehouseId(WAREHOUSE_ID).setName("B");
            employeeRepository.AddEmployees(new List<Employee>() { employeeBuilderA.CreateEmployee(), employeeBuilderB.CreateEmployee() });
            var result = employeeController.Get(WAREHOUSE_ID).Employees.ToList();

            var correctEmployeeA = employeeBuilderA.CreateEmployee();
            var correctEmployeeB = employeeBuilderB.CreateEmployee();

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeA, result.First()));
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeB, result.Last()));
        }

        [Test]
        public void TestGetNonExistentEmployee()
        {
            onSetUp();
            try
            {
                var employeeResponse = employeeController.GetByName(NAME);
                var test = employeeResponse.Employees.Count();

                // Assert.AreEqual(employeeResponse.Employees, new EmployeeResponse().Employees);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [Test]
        public void TestGetEmployeeInNonexistentWarehouse()
        {
            onSetUp();
            try
            {
                var employees = employeeController.Get(WAREHOUSE_ID).Employees.ToList();
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(WAREHOUSE_ID.ToString()));
            }
        }

        [Test]
        public void TestAddEmployees()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            var addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();

            var response = employeeController.Post(addEmployeesRequest);
            var databaseEmployee = employeeRepository.GetEmployeesByName(NAME).First();
            var correctDatabaseEmploye = employeeBuilder.CreateEmployee();

            Assert.IsTrue(response.Success);
            Assert.IsTrue(EmployeesAreEqual(new Employee(databaseEmployee), correctDatabaseEmploye));
        }

        [Test]
        public void TestDeleteEmployees()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() { employeeBuilder.CreateEmployee() });
            
            var removeEmployeeRequest = new RemoveEmployeeRequest() { Name = NAME };
            employeeController.Delete(removeEmployeeRequest);

            try
            {
                employeeController.GetByName(NAME);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [Test]
        public void TestDeleteNonexistentEmployee()
        {
            onSetUp();
            var removeEmployeeRequest = new RemoveEmployeeRequest() { Name = NAME };

            try
            {
                employeeController.Delete(removeEmployeeRequest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [Test]
        public void TestAddDuplicateEmployee()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            var firstEmployee = employeeRepository.AddEmployees(new List<Employee>() { employeeBuilder.CreateEmployee() }).First();
            var addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();
            var secondEmployee = employeeController.Post(addEmployeesRequest).Employees.First();
            
            Assert.IsTrue(firstEmployee.Name == secondEmployee.Name);
            Assert.IsTrue(firstEmployee.ext == secondEmployee.ext);
            Assert.IsTrue(firstEmployee.role == secondEmployee.role);
            Assert.IsTrue(firstEmployee.WarehouseId == secondEmployee.WarehouseId);
            Assert.False(firstEmployee.Id == secondEmployee.Id);
        }

        private bool EmployeesAreEqual(Employee A, Employee B)
        {
            return A.WarehouseId == B.WarehouseId
                   && A.Name == B.Name
                   && A.role == B.role
                   && A.ext == B.ext;
        }
    }
}
