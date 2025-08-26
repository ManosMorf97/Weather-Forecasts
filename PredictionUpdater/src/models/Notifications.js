const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('Notifications', {
    Site_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Predictions',
        key: 'Timeslot_Id'
      }
    },
    City_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Predictions',
        key: 'Timeslot_Id'
      }
    },
    Timeslot_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Predictions',
        key: 'Timeslot_Id'
      }
    },
    Description: {
      type: DataTypes.TEXT,
      allowNull: true
    },
    DateNotification: {
      type: DataTypes.DATEONLY,
      allowNull: false,
      defaultValue: "0001-01-01"
    },
    TimeNotification: {
      type: DataTypes.TIME,
      allowNull: false,
      defaultValue: "00:00:00"
    }
  }, {
    sequelize,
    tableName: 'Notifications',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_Notifications_City_Id_Site_Id_Timeslot_Id",
        unique: true,
        fields: [
          { name: "City_Id" },
          { name: "Site_Id" },
          { name: "Timeslot_Id" },
        ]
      },
      {
        name: "PK_Notifications",
        unique: true,
        fields: [
          { name: "Site_Id" },
          { name: "City_Id" },
          { name: "Timeslot_Id" },
        ]
      },
    ]
  });
};
